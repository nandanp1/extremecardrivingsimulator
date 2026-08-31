#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace RacingConversion.Editor
{
    /// <summary>
    /// Converts AssetRipper's legacy Unity static-batch representation in 1Menu
    /// into ordinary per-renderer meshes that current Unity versions can build.
    ///
    /// The old scene stores a byte-packed list of combined-mesh submesh indices
    /// in MeshRenderer.m_SubsetIndices. Unity 6 reads the scene but drops that
    /// legacy field when it serializes/builds it, leaving the renderers with a
    /// zero-length static batch. This tool captures the raw YAML metadata before
    /// opening/saving the scene, extracts only the selected combined submeshes,
    /// and converts their batch/world-space vertices back to renderer-local space.
    /// </summary>
    internal static class LegacyStaticBatchMigration
    {
        private const string ScenePath = "Assets/Scenes/Common/1Menu.unity";
        private const string CombinedMeshGuid = "69a8f9474e53bc34ea76c241ef17d371";
        private const string GeneratedRoot =
            "Assets/Reconstruction/Generated/1MenuLegacyStaticBatch";
        private const string GeneratedMeshFolder = GeneratedRoot + "/Meshes";
        private const string ManifestPath = GeneratedRoot + "/LegacySubsetManifest.json";
        private const string BackupFolder = "MigrationBackups/1MenuLegacyStaticBatch";

        private static readonly long[] ProofRendererFileIds = { 2019, 2042 };

        private static readonly Regex DocumentHeader = new Regex(
            @"^--- !u!(?<classId>-?\d+) &(?<fileId>-?\d+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex ObjectReference = new Regex(
            @"\{fileID:\s*(?<fileId>-?\d+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex InlineVector4 = new Regex(
            @"^\{x:\s*(?<x>[^,]+),\s*y:\s*(?<y>[^,]+),\s*z:\s*(?<z>[^,]+),\s*w:\s*(?<w>[^}]+)\}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [MenuItem("Racing Conversion/Legacy Static Batches/Analyze 1Menu YAML")]
        public static void AnalyzeMenuYaml()
        {
            string absoluteScenePath = ToAbsoluteProjectPath(ScenePath);
            string rawYaml = File.ReadAllText(absoluteScenePath);
            List<RendererSubsetRecord> records = ParseRawScene(rawYaml);

            Debug.Log(string.Format(
                CultureInfo.InvariantCulture,
                "[LegacyStaticBatch] 1Menu raw YAML contains {0} renderers with " +
                "non-empty m_SubsetIndices ({1} selected submeshes total).",
                records.Count,
                records.Sum(record => record.submeshes.Length)));

            foreach (RendererSubsetRecord proof in records.Where(
                         record => ProofRendererFileIds.Contains(record.rendererFileId)))
            {
                Debug.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatch] proof record renderer={0}, gameObject={1}, " +
                    "name={2}, submeshes=[{3}]",
                    proof.rendererFileId,
                    proof.gameObjectFileId,
                    proof.gameObjectName,
                    string.Join(",", proof.submeshes)));
            }
        }

        [MenuItem("Racing Conversion/Legacy Static Batches/Migrate Proof (Suelo + Plataforma001)")]
        public static void MigrateProof()
        {
            RunMigration(MigrationScope.Proof);
        }

        [MenuItem("Racing Conversion/Legacy Static Batches/Migrate All 1Menu Renderers")]
        public static void MigrateAll()
        {
            RunMigration(MigrationScope.All);
        }

        // Stable batch-mode entry points used by the reconstruction audit.
        public static void MigrateProofFromCommandLine()
        {
            RunMigration(MigrationScope.Proof);
        }

        public static void MigrateAllFromCommandLine()
        {
            RunMigration(MigrationScope.All);
        }

        private static void RunMigration(MigrationScope scope)
        {
            string absoluteScenePath = ToAbsoluteProjectPath(ScenePath);
            if (!File.Exists(absoluteScenePath))
            {
                throw new FileNotFoundException("1Menu scene was not found.", absoluteScenePath);
            }

            byte[] originalSceneBytes = File.ReadAllBytes(absoluteScenePath);
            string sourceHash = ComputeSha256(originalSceneBytes);
            string sceneGuidBefore = AssetDatabase.AssetPathToGUID(ScenePath);
            if (string.IsNullOrEmpty(sceneGuidBefore))
            {
                throw new InvalidOperationException("1Menu has no AssetDatabase GUID.");
            }

            // This happens before OpenScene and, critically, before SaveScene.
            string backupPath = EnsureRawSceneBackup(absoluteScenePath, sourceHash);

            List<RendererSubsetRecord> rawRecords = ParseRawScene(
                Encoding.UTF8.GetString(originalSceneBytes));
            LegacySubsetManifest manifest = MergeAndPersistManifest(
                rawRecords,
                sceneGuidBefore,
                sourceHash,
                backupPath);

            List<RendererSubsetRecord> requestedRecords = scope == MigrationScope.Proof
                ? manifest.renderers
                    .Where(record => ProofRendererFileIds.Contains(record.rendererFileId))
                    .OrderBy(record => record.rendererFileId)
                    .ToList()
                : manifest.renderers.OrderBy(record => record.rendererFileId).ToList();

            if (scope == MigrationScope.Proof && requestedRecords.Count != ProofRendererFileIds.Length)
            {
                throw new InvalidOperationException(
                    "The subset manifest does not contain both proof renderers 2019 and 2042.");
            }

            if (requestedRecords.Count == 0)
            {
                throw new InvalidOperationException("No legacy static-batch records were found.");
            }

            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[LegacyStaticBatch] Migration cancelled; open scene changes were not saved.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Dictionary<long, MeshRenderer> renderersByFileId = IndexSceneRenderers(scene);
            Dictionary<Mesh, SourceMeshSnapshot> snapshots = new Dictionary<Mesh, SourceMeshSnapshot>();
            List<StagedMigration> staged = new List<StagedMigration>();

            try
            {
                EnsureAssetFolder(GeneratedMeshFolder);

                // Stage every generated mesh before changing the scene. If any
                // validation/extraction fails, no renderer assignment is saved.
                foreach (RendererSubsetRecord record in requestedRecords)
                {
                    MeshRenderer renderer = ResolveRenderer(record, renderersByFileId);
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null)
                    {
                        throw new InvalidOperationException(
                            "Renderer " + record.rendererFileId + " has no MeshFilter.");
                    }

                    string generatedPath = GetGeneratedMeshPath(record);
                    string currentMeshPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                    if (string.Equals(currentMeshPath, generatedPath, StringComparison.Ordinal))
                    {
                        ValidateAlreadyMigrated(renderer, filter.sharedMesh, record, generatedPath);
                        staged.Add(StagedMigration.AlreadyComplete(record, renderer, filter, generatedPath));
                        continue;
                    }

                    Mesh sourceMesh = filter.sharedMesh;
                    if (sourceMesh == null)
                    {
                        throw new InvalidOperationException(
                            "Renderer " + record.rendererFileId + " has no source mesh.");
                    }

                    string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
                    string sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
                    if (!string.Equals(sourceGuid, CombinedMeshGuid, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} ({1}) references {2} ({3}), not the expected " +
                            "legacy combined mesh {4}.",
                            record.rendererFileId,
                            record.gameObjectName,
                            sourcePath,
                            sourceGuid,
                            CombinedMeshGuid));
                    }

                    if (renderer.sharedMaterials.Length != record.submeshes.Length)
                    {
                        throw new InvalidOperationException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} ({1}) has {2} materials but selects {3} submeshes. " +
                            "Automatic material remapping would be unsafe.",
                            record.rendererFileId,
                            record.gameObjectName,
                            renderer.sharedMaterials.Length,
                            record.submeshes.Length));
                    }

                    SourceMeshSnapshot snapshot;
                    if (!snapshots.TryGetValue(sourceMesh, out snapshot))
                    {
                        snapshot = new SourceMeshSnapshot(sourceMesh, sourcePath);
                        snapshots.Add(sourceMesh, snapshot);
                        Debug.Log(snapshot.DescribeLayout());
                    }

                    ExtractionResult extraction = ExtractRendererMesh(
                        snapshot,
                        renderer.transform,
                        record);
                    Mesh generatedMesh = PersistGeneratedMesh(extraction.mesh, generatedPath);

                    staged.Add(new StagedMigration(
                        record,
                        renderer,
                        filter,
                        generatedMesh,
                        generatedPath,
                        extraction));
                }

                int changedCount = 0;
                foreach (StagedMigration migration in staged)
                {
                    StaticEditorFlags flagsBefore =
                        GameObjectUtility.GetStaticEditorFlags(migration.renderer.gameObject);
                    StaticEditorFlags flagsAfter = flagsBefore & ~StaticEditorFlags.BatchingStatic;
                    int lodReferenceCount = CountLodReferences(scene, migration.renderer);
                    Material[] materialsBefore = migration.renderer.sharedMaterials;

                    bool needsSceneUpdate =
                        !migration.alreadyComplete ||
                        migration.renderer.lightmapIndex != migration.record.lightmapIndex ||
                        migration.renderer.lightmapScaleOffset !=
                        migration.record.lightmapScaleOffset ||
                        flagsBefore != flagsAfter;
                    if (!needsSceneUpdate)
                    {
                        Debug.Log(string.Format(
                            CultureInfo.InvariantCulture,
                            "[LegacyStaticBatch] already migrated renderer={0}, name={1}, " +
                            "lightmapIndex={2}, lightmapST={3}, asset={4}",
                            migration.record.rendererFileId,
                            migration.record.gameObjectName,
                            migration.record.lightmapIndex,
                            migration.record.lightmapScaleOffset,
                            migration.generatedPath));
                        continue;
                    }

                    if (!migration.alreadyComplete)
                    {
                        migration.filter.sharedMesh = migration.generatedMesh;
                    }
                    ClearLegacyStaticBatchState(migration.renderer);

                    GameObjectUtility.SetStaticEditorFlags(
                        migration.renderer.gameObject,
                        flagsAfter);

                    // Unity 6 also fails to carry the old renderer lightmap fields
                    // through this reconstructed scene. Restore the exact values
                    // captured from the original raw YAML, not the already-lost
                    // values exposed by the loaded MeshRenderer.
                    migration.renderer.lightmapIndex = migration.record.lightmapIndex;
                    migration.renderer.lightmapScaleOffset =
                        migration.record.lightmapScaleOffset;

                    // The renderer component is intentionally retained, so LODGroup,
                    // material, and script references remain untouched.
                    ValidatePreservedRendererState(
                        scene,
                        migration.renderer,
                        materialsBefore,
                        migration.record.lightmapIndex,
                        migration.record.lightmapScaleOffset,
                        lodReferenceCount);

                    EditorUtility.SetDirty(migration.filter);
                    EditorUtility.SetDirty(migration.renderer);
                    EditorUtility.SetDirty(migration.renderer.gameObject);
                    changedCount++;

                    Debug.Log(migration.alreadyComplete
                        ? migration.DescribeMetadataRepair(flagsBefore, flagsAfter, lodReferenceCount)
                        : migration.DescribeResult(flagsBefore, flagsAfter, lodReferenceCount));
                }

                if (changedCount > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    AssetDatabase.SaveAssets();
                    if (!EditorSceneManager.SaveScene(scene, ScenePath, false))
                    {
                        throw new IOException("Unity failed to save " + ScenePath);
                    }
                    AssetDatabase.SaveAssets();
                }

                string sceneGuidAfter = AssetDatabase.AssetPathToGUID(ScenePath);
                if (!string.Equals(sceneGuidBefore, sceneGuidAfter, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "1Menu GUID changed unexpectedly: {0} -> {1}",
                        sceneGuidBefore,
                        sceneGuidAfter));
                }

                Debug.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatch] {0} migration complete: requested={1}, changed={2}, " +
                    "unchanged={3}, sceneGuid={4}, rawBackup={5}, manifest={6}",
                    scope,
                    staged.Count,
                    changedCount,
                    staged.Count - changedCount,
                    sceneGuidAfter,
                    backupPath,
                    ManifestPath));
            }
            catch
            {
                Debug.LogError(
                    "[LegacyStaticBatch] Migration failed before a successful scene save. " +
                    "The exact pre-migration YAML is preserved at " + backupPath);
                throw;
            }
        }

        private static ExtractionResult ExtractRendererMesh(
            SourceMeshSnapshot source,
            Transform rendererTransform,
            RendererSubsetRecord record,
            string meshNamePrefix = "1Menu")
        {
            foreach (int submesh in record.submeshes)
            {
                if (submesh < 0 || submesh >= source.subMeshCount)
                {
                    throw new ArgumentOutOfRangeException(
                        "record.submeshes",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} requests submesh {1}, but {2} has {3} submeshes.",
                            record.rendererFileId,
                            submesh,
                            source.assetPath,
                            source.subMeshCount));
                }
            }

            var oldToNew = new Dictionary<int, int>();
            var newToOld = new List<int>();
            var outputIndices = new List<int[]>(record.submeshes.Length);
            var topologies = new List<MeshTopology>(record.submeshes.Length);
            int totalIndexCount = 0;

            foreach (int submesh in record.submeshes)
            {
                int[] sourceIndices = source.GetIndices(submesh);
                int[] remapped = new int[sourceIndices.Length];
                for (int index = 0; index < sourceIndices.Length; index++)
                {
                    int sourceVertex = sourceIndices[index];
                    if (sourceVertex < 0 || sourceVertex >= source.vertexCount)
                    {
                        throw new LegacyStaticBatchDataException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Submesh {0} contains invalid vertex index {1}/{2}.",
                            submesh,
                            sourceVertex,
                            source.vertexCount));
                    }

                    int compactVertex;
                    if (!oldToNew.TryGetValue(sourceVertex, out compactVertex))
                    {
                        compactVertex = newToOld.Count;
                        oldToNew.Add(sourceVertex, compactVertex);
                        newToOld.Add(sourceVertex);
                    }

                    remapped[index] = compactVertex;
                }

                outputIndices.Add(remapped);
                topologies.Add(source.GetTopology(submesh));
                totalIndexCount += remapped.Length;
            }

            Mesh output = new Mesh
            {
                name = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}_{1}_{2}_LegacyBatchRecovered",
                    meshNamePrefix,
                    record.rendererFileId,
                    SanitizeFileName(record.gameObjectName)),
                indexFormat = newToOld.Count > ushort.MaxValue
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };

            output.SetVertexBufferParams(newToOld.Count, source.vertexAttributes);
            source.ValidateDestinationLayout(output);
            Dictionary<int, byte[]> compactStreams =
                source.CreateCompactedDestinationStreams(output, newToOld);

            Matrix4x4 worldToLocal = rendererTransform.worldToLocalMatrix;
            Matrix4x4 normalWorldToLocal = rendererTransform.localToWorldMatrix.transpose;
            float handedness = worldToLocal.determinant < 0.0f ? -1.0f : 1.0f;
            Bounds sourceWorldBounds = new Bounds();
            Bounds outputLocalBounds = new Bounds();
            Bounds roundTripWorldBounds = new Bounds();
            bool hasBounds = false;
            float maxRoundTripError = 0.0f;

            for (int vertex = 0; vertex < newToOld.Count; vertex++)
            {
                Vector4 sourcePosition = source.ReadAttribute(
                    output,
                    compactStreams,
                    VertexAttribute.Position,
                    vertex);
                Vector3 worldPosition = new Vector3(
                    sourcePosition.x,
                    sourcePosition.y,
                    sourcePosition.z);
                Vector3 localPosition = worldToLocal.MultiplyPoint3x4(worldPosition);
                source.WriteAttribute(
                    output,
                    compactStreams,
                    VertexAttribute.Position,
                    vertex,
                    new Vector4(localPosition.x, localPosition.y, localPosition.z, sourcePosition.w));

                if (source.HasAttribute(VertexAttribute.Normal))
                {
                    Vector4 packedNormal = source.ReadAttribute(
                        output,
                        compactStreams,
                        VertexAttribute.Normal,
                        vertex);
                    Vector3 localNormal = normalWorldToLocal.MultiplyVector(
                        new Vector3(packedNormal.x, packedNormal.y, packedNormal.z)).normalized;
                    source.WriteAttribute(
                        output,
                        compactStreams,
                        VertexAttribute.Normal,
                        vertex,
                        new Vector4(localNormal.x, localNormal.y, localNormal.z, packedNormal.w));
                }

                if (source.HasAttribute(VertexAttribute.Tangent))
                {
                    Vector4 packedTangent = source.ReadAttribute(
                        output,
                        compactStreams,
                        VertexAttribute.Tangent,
                        vertex);
                    Vector3 localTangent = worldToLocal.MultiplyVector(
                        new Vector3(packedTangent.x, packedTangent.y, packedTangent.z)).normalized;
                    source.WriteAttribute(
                        output,
                        compactStreams,
                        VertexAttribute.Tangent,
                        vertex,
                        new Vector4(
                            localTangent.x,
                            localTangent.y,
                            localTangent.z,
                            packedTangent.w * handedness));
                }

                Vector3 roundTripWorld = rendererTransform.localToWorldMatrix.MultiplyPoint3x4(
                    localPosition);
                maxRoundTripError = Mathf.Max(
                    maxRoundTripError,
                    Vector3.Distance(worldPosition, roundTripWorld));

                if (!hasBounds)
                {
                    sourceWorldBounds = new Bounds(worldPosition, Vector3.zero);
                    outputLocalBounds = new Bounds(localPosition, Vector3.zero);
                    roundTripWorldBounds = new Bounds(roundTripWorld, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    sourceWorldBounds.Encapsulate(worldPosition);
                    outputLocalBounds.Encapsulate(localPosition);
                    roundTripWorldBounds.Encapsulate(roundTripWorld);
                }
            }

            foreach (KeyValuePair<int, byte[]> stream in compactStreams)
            {
                var nativeBytes = new NativeArray<byte>(
                    stream.Value.Length,
                    Allocator.Temp,
                    NativeArrayOptions.UninitializedMemory);
                try
                {
                    for (int index = 0; index < stream.Value.Length; index++)
                    {
                        nativeBytes[index] = stream.Value[index];
                    }

                    output.SetVertexBufferData(
                        nativeBytes,
                        0,
                        0,
                        nativeBytes.Length,
                        stream.Key,
                        MeshUpdateFlags.DontRecalculateBounds |
                        MeshUpdateFlags.DontValidateIndices);
                }
                finally
                {
                    nativeBytes.Dispose();
                }
            }

            output.subMeshCount = outputIndices.Count;
            for (int submesh = 0; submesh < outputIndices.Count; submesh++)
            {
                output.SetIndices(
                    outputIndices[submesh],
                    topologies[submesh],
                    submesh,
                    false,
                    0);
            }

            output.bindposes = source.bindposes;
            output.bounds = outputLocalBounds;

            return new ExtractionResult(
                output,
                newToOld.Count,
                totalIndexCount,
                sourceWorldBounds,
                outputLocalBounds,
                roundTripWorldBounds,
                maxRoundTripError);
        }

        private static Mesh PersistGeneratedMesh(Mesh generated, string assetPath)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, assetPath);
                return generated;
            }

            EditorUtility.CopySerialized(generated, existing);
            UnityEngine.Object.DestroyImmediate(generated);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void ClearLegacyStaticBatchState(MeshRenderer renderer)
        {
            var serializedRenderer = new SerializedObject(renderer);

            SerializedProperty subsetIndices =
                serializedRenderer.FindProperty("m_SubsetIndices");
            if (subsetIndices != null && subsetIndices.isArray)
            {
                subsetIndices.arraySize = 0;
            }

            SerializedProperty batchRoot =
                serializedRenderer.FindProperty("m_StaticBatchRoot");
            if (batchRoot != null && batchRoot.propertyType == SerializedPropertyType.ObjectReference)
            {
                batchRoot.objectReferenceValue = null;
            }

            SerializedProperty batchInfo =
                serializedRenderer.FindProperty("m_StaticBatchInfo");
            if (batchInfo != null)
            {
                SerializedProperty firstSubMesh = batchInfo.FindPropertyRelative("firstSubMesh");
                SerializedProperty subMeshCount = batchInfo.FindPropertyRelative("subMeshCount");
                if (firstSubMesh != null)
                {
                    firstSubMesh.intValue = 0;
                }

                if (subMeshCount != null)
                {
                    subMeshCount.intValue = 0;
                }
            }

            serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ValidatePreservedRendererState(
            Scene scene,
            MeshRenderer renderer,
            Material[] materialsBefore,
            int lightmapIndex,
            Vector4 lightmapScaleOffset,
            int lodReferenceCount)
        {
            if (!materialsBefore.SequenceEqual(renderer.sharedMaterials))
            {
                throw new InvalidOperationException(
                    "Material references changed for " + renderer.name + ".");
            }

            if (renderer.lightmapIndex != lightmapIndex ||
                renderer.lightmapScaleOffset != lightmapScaleOffset)
            {
                throw new InvalidOperationException(
                    "Lightmap binding changed for " + renderer.name + ".");
            }

            if (CountLodReferences(scene, renderer) != lodReferenceCount)
            {
                throw new InvalidOperationException(
                    "LODGroup renderer references changed for " + renderer.name + ".");
            }
        }

        private static void ValidateAlreadyMigrated(
            MeshRenderer renderer,
            Mesh mesh,
            RendererSubsetRecord record,
            string assetPath)
        {
            if (mesh == null || mesh.subMeshCount != record.submeshes.Length)
            {
                throw new LegacyStaticBatchDataException(
                    "Existing generated mesh is invalid: " + assetPath);
            }

            if (renderer.sharedMaterials.Length != mesh.subMeshCount)
            {
                throw new LegacyStaticBatchDataException(
                    "Existing generated mesh/material count mismatch: " + assetPath);
            }
        }

        private static MeshRenderer ResolveRenderer(
            RendererSubsetRecord record,
            Dictionary<long, MeshRenderer> renderersByFileId)
        {
            MeshRenderer renderer;
            if (renderersByFileId.TryGetValue(record.rendererFileId, out renderer))
            {
                return renderer;
            }

            // A scene reserialize should preserve local IDs. This name fallback is
            // intentionally strict and exists only to produce a useful diagnosis.
            MeshRenderer[] nameMatches = renderersByFileId.Values
                .Where(candidate => candidate.gameObject.name == record.gameObjectName)
                .ToArray();
            if (nameMatches.Length == 1)
            {
                Debug.LogWarning(string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatch] Renderer local ID {0} was not found; using the sole " +
                    "scene renderer named {1}.",
                    record.rendererFileId,
                    record.gameObjectName));
                return nameMatches[0];
            }

            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                "Could not uniquely resolve renderer {0} ({1}); nameMatches={2}.",
                record.rendererFileId,
                record.gameObjectName,
                nameMatches.Length));
        }

        private static Dictionary<long, MeshRenderer> IndexSceneRenderers(Scene scene)
        {
            var result = new Dictionary<long, MeshRenderer>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    string guid;
                    long localId;
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                            renderer,
                            out guid,
                            out localId))
                    {
                        GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(renderer);
                        localId = unchecked((long)globalId.targetObjectId);
                    }

                    if (localId == 0)
                    {
                        // A few reconstructed hierarchy objects are transient from
                        // Unity's point of view. They have no YAML document ID and
                        // therefore cannot correspond to an m_SubsetIndices record.
                        Debug.LogWarning(
                            "[LegacyStaticBatch] Skipping non-persistent renderer without a " +
                            "scene local ID: " + renderer.name);
                        continue;
                    }

                    MeshRenderer duplicate;
                    if (result.TryGetValue(localId, out duplicate))
                    {
                        throw new InvalidOperationException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Duplicate scene renderer local ID {0}: {1} and {2}.",
                            localId,
                            duplicate.name,
                            renderer.name));
                    }

                    result.Add(localId, renderer);
                }
            }

            return result;
        }

        private static int CountLodReferences(Scene scene, Renderer renderer)
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (LODGroup group in root.GetComponentsInChildren<LODGroup>(true))
                {
                    foreach (LOD lod in group.GetLODs())
                    {
                        count += lod.renderers.Count(candidate => candidate == renderer);
                    }
                }
            }

            return count;
        }

        private static LegacySubsetManifest MergeAndPersistManifest(
            List<RendererSubsetRecord> rawRecords,
            string sceneGuid,
            string sourceHash,
            string backupPath)
        {
            LegacySubsetManifest existing = null;
            string absoluteManifestPath = ToAbsoluteProjectPath(ManifestPath);
            if (File.Exists(absoluteManifestPath))
            {
                existing = JsonUtility.FromJson<LegacySubsetManifest>(
                    File.ReadAllText(absoluteManifestPath));
            }

            var merged = new Dictionary<long, RendererSubsetRecord>();
            if (existing != null && existing.renderers != null)
            {
                foreach (RendererSubsetRecord record in existing.renderers)
                {
                    merged[record.rendererFileId] = record;
                }
            }

            // The current scene no longer contains m_SubsetIndices after the
            // first Unity 6 save. Rehydrate the authoritative records (including
            // original lightmap values) from the exact raw backup before merging
            // whatever metadata may still exist in the current YAML.
            if (existing != null &&
                !string.IsNullOrEmpty(existing.originalRawBackupPath) &&
                File.Exists(existing.originalRawBackupPath))
            {
                foreach (RendererSubsetRecord record in ParseRawScene(
                             File.ReadAllText(existing.originalRawBackupPath)))
                {
                    merged[record.rendererFileId] = record;
                }
            }

            foreach (RendererSubsetRecord record in rawRecords)
            {
                merged[record.rendererFileId] = record;
            }

            var manifest = new LegacySubsetManifest
            {
                scenePath = ScenePath,
                sceneGuid = sceneGuid,
                originalSourceSha256 = existing != null &&
                                       !string.IsNullOrEmpty(existing.originalSourceSha256)
                    ? existing.originalSourceSha256
                    : sourceHash,
                originalRawBackupPath = existing != null &&
                                        !string.IsNullOrEmpty(existing.originalRawBackupPath)
                    ? existing.originalRawBackupPath
                    : backupPath,
                renderers = merged.Values.OrderBy(record => record.rendererFileId).ToList()
            };

            if (manifest.renderers.Count == 0)
            {
                throw new LegacyStaticBatchDataException(
                    "Neither the raw scene nor the existing manifest contains subset records.");
            }

            EnsureAssetFolder(GeneratedRoot);
            File.WriteAllText(
                absoluteManifestPath,
                JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(ManifestPath, ImportAssetOptions.ForceSynchronousImport);

            Debug.Log(string.Format(
                CultureInfo.InvariantCulture,
                "[LegacyStaticBatch] Preserved {0} legacy renderer records in {1}.",
                manifest.renderers.Count,
                ManifestPath));
            return manifest;
        }

        private static List<RendererSubsetRecord> ParseRawScene(string rawYaml)
        {
            var gameObjectNames = new Dictionary<long, string>();
            var pendingRenderers = new List<PendingRendererRecord>();
            int currentClassId = 0;
            long currentFileId = 0;
            long currentGameObjectId = 0;
            string currentName = null;
            string currentSubsetHex = null;
            int currentSourceLightmapIndex = -1;
            Vector4 currentLightmapScaleOffset = new Vector4(1, 1, 0, 0);
            bool hasCurrentLightmapIndex = false;
            bool hasCurrentLightmapScaleOffset = false;

            Action flushDocument = () =>
            {
                if (currentClassId == 1 && currentFileId != 0 && currentName != null)
                {
                    gameObjectNames[currentFileId] = currentName;
                }
                else if (currentClassId == 23 &&
                         currentFileId != 0 &&
                         currentGameObjectId != 0 &&
                         !string.IsNullOrWhiteSpace(currentSubsetHex))
                {
                    if (!hasCurrentLightmapIndex || !hasCurrentLightmapScaleOffset)
                    {
                        throw new LegacyStaticBatchDataException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} has subset metadata but incomplete lightmap metadata.",
                            currentFileId));
                    }

                    pendingRenderers.Add(new PendingRendererRecord
                    {
                        rendererFileId = currentFileId,
                        gameObjectFileId = currentGameObjectId,
                        subsetHex = currentSubsetHex.Trim(),
                        sourceLightmapIndex = currentSourceLightmapIndex,
                        lightmapScaleOffset = currentLightmapScaleOffset
                    });
                }
            };

            using (var reader = new StringReader(rawYaml))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match header = DocumentHeader.Match(line);
                    if (header.Success)
                    {
                        flushDocument();
                        currentClassId = int.Parse(
                            header.Groups["classId"].Value,
                            CultureInfo.InvariantCulture);
                        currentFileId = long.Parse(
                            header.Groups["fileId"].Value,
                            CultureInfo.InvariantCulture);
                        currentGameObjectId = 0;
                        currentName = null;
                        currentSubsetHex = null;
                        currentSourceLightmapIndex = -1;
                        currentLightmapScaleOffset = new Vector4(1, 1, 0, 0);
                        hasCurrentLightmapIndex = false;
                        hasCurrentLightmapScaleOffset = false;
                        continue;
                    }

                    if (currentClassId == 1 && line.StartsWith("  m_Name: ", StringComparison.Ordinal))
                    {
                        currentName = UnquoteYamlScalar(line.Substring("  m_Name: ".Length));
                    }
                    else if (currentClassId == 23 &&
                             line.StartsWith("  m_GameObject: ", StringComparison.Ordinal))
                    {
                        Match reference = ObjectReference.Match(line);
                        if (reference.Success)
                        {
                            currentGameObjectId = long.Parse(
                                reference.Groups["fileId"].Value,
                                CultureInfo.InvariantCulture);
                        }
                    }
                    else if (currentClassId == 23 &&
                             line.StartsWith("  m_SubsetIndices:", StringComparison.Ordinal))
                    {
                        currentSubsetHex = line.Substring("  m_SubsetIndices:".Length).Trim();
                    }
                    else if (currentClassId == 23 &&
                             line.StartsWith("  m_LightmapIndex:", StringComparison.Ordinal))
                    {
                        currentSourceLightmapIndex = int.Parse(
                            line.Substring("  m_LightmapIndex:".Length).Trim(),
                            CultureInfo.InvariantCulture);
                        hasCurrentLightmapIndex = true;
                    }
                    else if (currentClassId == 23 &&
                             line.StartsWith("  m_LightmapTilingOffset:", StringComparison.Ordinal))
                    {
                        currentLightmapScaleOffset = ParseInlineVector4(
                            line.Substring("  m_LightmapTilingOffset:".Length).Trim());
                        hasCurrentLightmapScaleOffset = true;
                    }
                }
            }

            flushDocument();

            var records = new List<RendererSubsetRecord>(pendingRenderers.Count);
            foreach (PendingRendererRecord pending in pendingRenderers)
            {
                string name;
                if (!gameObjectNames.TryGetValue(pending.gameObjectFileId, out name))
                {
                    name = "GameObject_" + pending.gameObjectFileId.ToString(CultureInfo.InvariantCulture);
                }

                records.Add(new RendererSubsetRecord
                {
                    rendererFileId = pending.rendererFileId,
                    gameObjectFileId = pending.gameObjectFileId,
                    gameObjectName = name,
                    subsetHexLittleEndian = pending.subsetHex,
                    submeshes = DecodeLittleEndianInt32Array(pending.subsetHex),
                    sourceLightmapIndex = pending.sourceLightmapIndex,
                    lightmapIndex = pending.sourceLightmapIndex == 255
                        ? -1
                        : pending.sourceLightmapIndex,
                    lightmapScaleOffset = pending.lightmapScaleOffset
                });
            }

            return records.OrderBy(record => record.rendererFileId).ToList();
        }

        private static int[] DecodeLittleEndianInt32Array(string hex)
        {
            if (hex.Length == 0 || hex.Length % 8 != 0)
            {
                throw new LegacyStaticBatchDataException(
                    "m_SubsetIndices must contain complete little-endian Int32 values: " + hex);
            }

            var result = new int[hex.Length / 8];
            for (int index = 0; index < result.Length; index++)
            {
                int offset = index * 8;
                uint value =
                    ParseHexByte(hex, offset) |
                    ((uint)ParseHexByte(hex, offset + 2) << 8) |
                    ((uint)ParseHexByte(hex, offset + 4) << 16) |
                    ((uint)ParseHexByte(hex, offset + 6) << 24);
                result[index] = unchecked((int)value);
            }

            return result;
        }

        private static Vector4 ParseInlineVector4(string value)
        {
            Match match = InlineVector4.Match(value);
            if (!match.Success)
            {
                throw new LegacyStaticBatchDataException(
                    "Could not parse inline Vector4: " + value);
            }

            return new Vector4(
                float.Parse(match.Groups["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["z"].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["w"].Value, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        private static uint ParseHexByte(string value, int offset)
        {
            return uint.Parse(
                value.Substring(offset, 2),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture);
        }

        private static string EnsureRawSceneBackup(string absoluteScenePath, string sourceHash)
        {
            string projectDirectory = GetProjectDirectory();
            string absoluteBackupDirectory = Path.Combine(projectDirectory, BackupFolder);
            Directory.CreateDirectory(absoluteBackupDirectory);
            string absoluteBackupPath = Path.Combine(
                absoluteBackupDirectory,
                "1Menu.before-legacy-static-batch." + sourceHash.Substring(0, 16) + ".unity");

            if (!File.Exists(absoluteBackupPath))
            {
                File.Copy(absoluteScenePath, absoluteBackupPath, false);
            }

            string backupHash = ComputeSha256(File.ReadAllBytes(absoluteBackupPath));
            if (!string.Equals(sourceHash, backupHash, StringComparison.Ordinal))
            {
                throw new IOException("Raw scene backup hash does not match its source.");
            }

            Debug.Log("[LegacyStaticBatch] Exact raw scene backup: " + absoluteBackupPath);
            return absoluteBackupPath;
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            string[] segments = assetFolder.Split('/');
            if (segments.Length == 0 || segments[0] != "Assets")
            {
                throw new ArgumentException("Generated path must be below Assets.", "assetFolder");
            }

            string current = "Assets";
            for (int index = 1; index < segments.Length; index++)
            {
                string child = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(child))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = child;
            }
        }

        private static string GetGeneratedMeshPath(RendererSubsetRecord record)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}_{2}.asset",
                GeneratedMeshFolder,
                record.rendererFileId,
                SanitizeFileName(record.gameObjectName));
        }

        private static string SanitizeFileName(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                builder.Append(char.IsLetterOrDigit(character) || character == '_' || character == '-'
                    ? character
                    : '_');
            }

            return builder.Length == 0 ? "Renderer" : builder.ToString();
        }

        private static string UnquoteYamlScalar(string value)
        {
            value = value.Trim();
            if (value.Length >= 2 &&
                ((value[0] == '\'' && value[value.Length - 1] == '\'') ||
                 (value[0] == '"' && value[value.Length - 1] == '"')))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            return Path.Combine(GetProjectDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string GetProjectDirectory()
        {
            string projectDirectory = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectDirectory))
            {
                throw new InvalidOperationException("Could not resolve the Unity project directory.");
            }

            return projectDirectory;
        }

        private static string ComputeSha256(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(value => value.ToString("x2")));
            }
        }

        private enum MigrationScope
        {
            Proof,
            All
        }

        /// <summary>
        /// Reusable façade for the validated raw-stream extractor. Gameplay scene
        /// migration uses this without changing the established 1Menu workflow.
        /// A cache retains one immutable source snapshot per combined mesh so a
        /// scene with hundreds of subset renderers reads each source buffer once.
        /// </summary>
        internal sealed class ReusableMeshExtractor
        {
            private readonly Dictionary<Mesh, SourceMeshSnapshot> snapshots =
                new Dictionary<Mesh, SourceMeshSnapshot>();

            internal ReusableExtractionResult Extract(
                Mesh sourceMesh,
                string sourceAssetPath,
                Transform rendererTransform,
                long rendererFileId,
                string gameObjectName,
                int[] submeshes,
                string meshNamePrefix)
            {
                if (sourceMesh == null)
                {
                    throw new ArgumentNullException("sourceMesh");
                }

                SourceMeshSnapshot snapshot;
                if (!snapshots.TryGetValue(sourceMesh, out snapshot))
                {
                    snapshot = new SourceMeshSnapshot(sourceMesh, sourceAssetPath);
                    snapshots.Add(sourceMesh, snapshot);
                    Debug.Log(snapshot.DescribeLayout());
                }

                var record = new RendererSubsetRecord
                {
                    rendererFileId = rendererFileId,
                    gameObjectName = gameObjectName,
                    submeshes = submeshes
                };
                ExtractionResult result = ExtractRendererMesh(
                    snapshot,
                    rendererTransform,
                    record,
                    meshNamePrefix);
                return new ReusableExtractionResult(
                    result.mesh,
                    result.vertexCount,
                    result.indexCount,
                    result.sourceWorldBounds,
                    result.outputLocalBounds,
                    result.roundTripWorldBounds,
                    result.maxRoundTripError);
            }
        }

        internal sealed class ReusableExtractionResult
        {
            internal readonly Mesh mesh;
            internal readonly int vertexCount;
            internal readonly int indexCount;
            internal readonly Bounds sourceWorldBounds;
            internal readonly Bounds outputLocalBounds;
            internal readonly Bounds roundTripWorldBounds;
            internal readonly float maxRoundTripError;

            internal ReusableExtractionResult(
                Mesh mesh,
                int vertexCount,
                int indexCount,
                Bounds sourceWorldBounds,
                Bounds outputLocalBounds,
                Bounds roundTripWorldBounds,
                float maxRoundTripError)
            {
                this.mesh = mesh;
                this.vertexCount = vertexCount;
                this.indexCount = indexCount;
                this.sourceWorldBounds = sourceWorldBounds;
                this.outputLocalBounds = outputLocalBounds;
                this.roundTripWorldBounds = roundTripWorldBounds;
                this.maxRoundTripError = maxRoundTripError;
            }
        }

        [Serializable]
        private sealed class LegacySubsetManifest
        {
            public string scenePath;
            public string sceneGuid;
            public string originalSourceSha256;
            public string originalRawBackupPath;
            public List<RendererSubsetRecord> renderers = new List<RendererSubsetRecord>();
        }

        [Serializable]
        private sealed class RendererSubsetRecord
        {
            public long rendererFileId;
            public long gameObjectFileId;
            public string gameObjectName;
            public string subsetHexLittleEndian;
            public int[] submeshes;
            public int sourceLightmapIndex;
            public int lightmapIndex;
            public Vector4 lightmapScaleOffset;
        }

        private sealed class PendingRendererRecord
        {
            public long rendererFileId;
            public long gameObjectFileId;
            public string subsetHex;
            public int sourceLightmapIndex;
            public Vector4 lightmapScaleOffset;
        }

        private sealed class StagedMigration
        {
            public readonly RendererSubsetRecord record;
            public readonly MeshRenderer renderer;
            public readonly MeshFilter filter;
            public readonly Mesh generatedMesh;
            public readonly string generatedPath;
            public readonly ExtractionResult extraction;
            public readonly bool alreadyComplete;

            public StagedMigration(
                RendererSubsetRecord record,
                MeshRenderer renderer,
                MeshFilter filter,
                Mesh generatedMesh,
                string generatedPath,
                ExtractionResult extraction)
            {
                this.record = record;
                this.renderer = renderer;
                this.filter = filter;
                this.generatedMesh = generatedMesh;
                this.generatedPath = generatedPath;
                this.extraction = extraction;
            }

            private StagedMigration(
                RendererSubsetRecord record,
                MeshRenderer renderer,
                MeshFilter filter,
                string generatedPath)
            {
                this.record = record;
                this.renderer = renderer;
                this.filter = filter;
                this.generatedPath = generatedPath;
                generatedMesh = filter.sharedMesh;
                alreadyComplete = true;
            }

            public static StagedMigration AlreadyComplete(
                RendererSubsetRecord record,
                MeshRenderer renderer,
                MeshFilter filter,
                string generatedPath)
            {
                return new StagedMigration(record, renderer, filter, generatedPath);
            }

            public string DescribeResult(
                StaticEditorFlags flagsBefore,
                StaticEditorFlags flagsAfter,
                int lodReferenceCount)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatch] migrated renderer={0}, gameObject={1}, name={2}, " +
                    "submeshes=[{3}], vertices={4}, indices={5}, materials={6}, " +
                    "lightmapIndex={7}, lightmapST={8}, lodRefs={9}, staticFlags={10}->{11}, " +
                    "sourceWorldBounds={12}, localBounds={13}, roundTripWorldBounds={14}, " +
                    "maxRoundTripError={15:R}, asset={16}",
                    record.rendererFileId,
                    record.gameObjectFileId,
                    record.gameObjectName,
                    string.Join(",", record.submeshes),
                    extraction.vertexCount,
                    extraction.indexCount,
                    renderer.sharedMaterials.Length,
                    record.lightmapIndex,
                    record.lightmapScaleOffset,
                    lodReferenceCount,
                    flagsBefore,
                    flagsAfter,
                    extraction.sourceWorldBounds,
                    extraction.outputLocalBounds,
                    extraction.roundTripWorldBounds,
                    extraction.maxRoundTripError,
                    generatedPath);
            }

            public string DescribeMetadataRepair(
                StaticEditorFlags flagsBefore,
                StaticEditorFlags flagsAfter,
                int lodReferenceCount)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatch] restored original renderer metadata renderer={0}, " +
                    "gameObject={1}, name={2}, lightmapIndex={3} (source={4}), " +
                    "lightmapST={5}, materials={6}, lodRefs={7}, staticFlags={8}->{9}, asset={10}",
                    record.rendererFileId,
                    record.gameObjectFileId,
                    record.gameObjectName,
                    record.lightmapIndex,
                    record.sourceLightmapIndex,
                    record.lightmapScaleOffset,
                    renderer.sharedMaterials.Length,
                    lodReferenceCount,
                    flagsBefore,
                    flagsAfter,
                    generatedPath);
            }
        }

        private sealed class ExtractionResult
        {
            public readonly Mesh mesh;
            public readonly int vertexCount;
            public readonly int indexCount;
            public readonly Bounds sourceWorldBounds;
            public readonly Bounds outputLocalBounds;
            public readonly Bounds roundTripWorldBounds;
            public readonly float maxRoundTripError;

            public ExtractionResult(
                Mesh mesh,
                int vertexCount,
                int indexCount,
                Bounds sourceWorldBounds,
                Bounds outputLocalBounds,
                Bounds roundTripWorldBounds,
                float maxRoundTripError)
            {
                this.mesh = mesh;
                this.vertexCount = vertexCount;
                this.indexCount = indexCount;
                this.sourceWorldBounds = sourceWorldBounds;
                this.outputLocalBounds = outputLocalBounds;
                this.roundTripWorldBounds = roundTripWorldBounds;
                this.maxRoundTripError = maxRoundTripError;
            }
        }

        private sealed class SourceVertexStream
        {
            public readonly int stride;
            public readonly byte[] bytes;

            public SourceVertexStream(int stride, byte[] bytes)
            {
                this.stride = stride;
                this.bytes = bytes;
            }
        }

        private sealed class SourceMeshSnapshot
        {
            private readonly Mesh source;
            private readonly Dictionary<int, int[]> indicesBySubmesh =
                new Dictionary<int, int[]>();
            private readonly Dictionary<int, MeshTopology> topologyBySubmesh =
                new Dictionary<int, MeshTopology>();
            private readonly Dictionary<VertexAttribute, VertexAttributeDescriptor> descriptorByAttribute;

            public readonly string assetPath;
            public readonly int vertexCount;
            public readonly int subMeshCount;
            public readonly VertexAttributeDescriptor[] vertexAttributes;
            public readonly Dictionary<int, SourceVertexStream> streams =
                new Dictionary<int, SourceVertexStream>();
            public readonly Matrix4x4[] bindposes;

            public SourceMeshSnapshot(Mesh source, string assetPath)
            {
                this.source = source;
                this.assetPath = assetPath;
                vertexCount = source.vertexCount;
                subMeshCount = source.subMeshCount;
                // Mesh.GetVertexAttributes returns semantic order, not physical
                // byte order (for this mesh Tangent is reported before UV0/UV1).
                // SetVertexBufferParams uses the descriptor order to lay out each
                // stream, so retain the source offsets explicitly here.
                vertexAttributes = source.GetVertexAttributes()
                    .OrderBy(descriptor => descriptor.stream)
                    .ThenBy(descriptor => source.GetVertexAttributeOffset(descriptor.attribute))
                    .ToArray();
                bindposes = source.bindposes;
                descriptorByAttribute = vertexAttributes.ToDictionary(
                    descriptor => descriptor.attribute,
                    descriptor => descriptor);

                if (!descriptorByAttribute.ContainsKey(VertexAttribute.Position))
                {
                    throw new LegacyStaticBatchDataException(assetPath + " has no position attribute.");
                }

                ValidateTransformAttribute(VertexAttribute.Position);
                if (HasAttribute(VertexAttribute.Normal))
                {
                    ValidateTransformAttribute(VertexAttribute.Normal);
                }

                if (HasAttribute(VertexAttribute.Tangent))
                {
                    ValidateTransformAttribute(VertexAttribute.Tangent);
                }

                if (source.blendShapeCount != 0)
                {
                    throw new NotSupportedException(
                        "The legacy combined mesh has blend shapes; compacting their delta " +
                        "streams requires an explicit migration path.");
                }

                using (Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(source))
                {
                    Mesh.MeshData meshData = meshDataArray[0];
                    foreach (int stream in vertexAttributes
                                 .Select(attribute => attribute.stream)
                                 .Distinct())
                    {
                        int stride = source.GetVertexBufferStride(stream);
                        int byteCount = checked(vertexCount * stride);
                        NativeArray<byte> nativeBytes = meshData.GetVertexData<byte>(stream);
                        if (nativeBytes.Length != byteCount)
                        {
                            throw new LegacyStaticBatchDataException(string.Format(
                                CultureInfo.InvariantCulture,
                                "Vertex stream {0} has {1} bytes; expected {2}.",
                                stream,
                                nativeBytes.Length,
                                byteCount));
                        }

                        byte[] managedBytes = new byte[byteCount];
                        for (int index = 0; index < byteCount; index++)
                        {
                            managedBytes[index] = nativeBytes[index];
                        }

                        streams.Add(stream, new SourceVertexStream(stride, managedBytes));
                    }

                    if (meshData.indexFormat == IndexFormat.UInt16)
                    {
                        NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
                        CaptureSubmeshIndices(meshData, indexData);
                    }
                    else
                    {
                        NativeArray<uint> indexData = meshData.GetIndexData<uint>();
                        CaptureSubmeshIndices(meshData, indexData);
                    }
                }
            }

            public int[] GetIndices(int submesh)
            {
                return indicesBySubmesh[submesh];
            }

            public MeshTopology GetTopology(int submesh)
            {
                return topologyBySubmesh[submesh];
            }

            public bool HasAttribute(VertexAttribute attribute)
            {
                return descriptorByAttribute.ContainsKey(attribute);
            }

            public Dictionary<int, byte[]> CreateCompactedDestinationStreams(
                Mesh destination,
                IList<int> newToOld)
            {
                var result = new Dictionary<int, byte[]>();
                foreach (int stream in vertexAttributes
                             .Select(descriptor =>
                                 destination.GetVertexAttributeStream(descriptor.attribute))
                             .Distinct())
                {
                    result.Add(
                        stream,
                        new byte[checked(newToOld.Count * destination.GetVertexBufferStride(stream))]);
                }

                // Unity is allowed to canonicalize attribute order when creating
                // a Mesh (it moves this source mesh's tangent ahead of its UVs).
                // Repack each semantic attribute into Unity's destination offset
                // instead of assuming the source and destination byte layouts match.
                foreach (VertexAttributeDescriptor descriptor in vertexAttributes)
                {
                    int destinationStream =
                        destination.GetVertexAttributeStream(descriptor.attribute);
                    int destinationStride = destination.GetVertexBufferStride(destinationStream);
                    int destinationOffset =
                        destination.GetVertexAttributeOffset(descriptor.attribute);
                    SourceVertexStream sourceStream = streams[descriptor.stream];
                    int sourceOffset = source.GetVertexAttributeOffset(descriptor.attribute);
                    int attributeByteCount = checked(
                        GetFormatSize(descriptor.format) * descriptor.dimension);

                    for (int newVertex = 0; newVertex < newToOld.Count; newVertex++)
                    {
                        Buffer.BlockCopy(
                            sourceStream.bytes,
                            checked(newToOld[newVertex] * sourceStream.stride + sourceOffset),
                            result[destinationStream],
                            checked(newVertex * destinationStride + destinationOffset),
                            attributeByteCount);
                    }
                }

                return result;
            }

            public Vector4 ReadAttribute(
                Mesh destination,
                Dictionary<int, byte[]> compactStreams,
                VertexAttribute attribute,
                int vertex)
            {
                VertexAttributeDescriptor descriptor = descriptorByAttribute[attribute];
                int stream = destination.GetVertexAttributeStream(attribute);
                byte[] bytes = compactStreams[stream];
                int stride = destination.GetVertexBufferStride(stream);
                int offset = checked(vertex * stride + destination.GetVertexAttributeOffset(attribute));
                var value = new Vector4();
                for (int component = 0; component < descriptor.dimension; component++)
                {
                    value[component] = ReadFloat(
                        bytes,
                        offset + component * GetFormatSize(descriptor.format),
                        descriptor.format);
                }

                return value;
            }

            public void WriteAttribute(
                Mesh destination,
                Dictionary<int, byte[]> compactStreams,
                VertexAttribute attribute,
                int vertex,
                Vector4 value)
            {
                VertexAttributeDescriptor descriptor = descriptorByAttribute[attribute];
                int stream = destination.GetVertexAttributeStream(attribute);
                byte[] bytes = compactStreams[stream];
                int stride = destination.GetVertexBufferStride(stream);
                int offset = checked(vertex * stride + destination.GetVertexAttributeOffset(attribute));
                for (int component = 0; component < descriptor.dimension; component++)
                {
                    WriteFloat(
                        bytes,
                        offset + component * GetFormatSize(descriptor.format),
                        descriptor.format,
                        value[component]);
                }
            }

            public void ValidateDestinationLayout(Mesh destination)
            {
                foreach (VertexAttributeDescriptor descriptor in vertexAttributes)
                {
                    if (!destination.HasVertexAttribute(descriptor.attribute) ||
                        destination.GetVertexAttributeFormat(descriptor.attribute) != descriptor.format ||
                        destination.GetVertexAttributeDimension(descriptor.attribute) != descriptor.dimension)
                    {
                        throw new InvalidOperationException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Unity changed the generated format for {0}: expected {1}x{2}, " +
                            "got {3}x{4}.",
                            descriptor.attribute,
                            descriptor.format,
                            descriptor.dimension,
                            destination.GetVertexAttributeFormat(descriptor.attribute),
                            destination.GetVertexAttributeDimension(descriptor.attribute)));
                    }
                }
            }

            public string DescribeLayout()
            {
                string layout = string.Join(
                    ", ",
                    vertexAttributes.Select(descriptor => string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:{1}x{2}@stream{3}+{4}",
                        descriptor.attribute,
                        descriptor.format,
                        descriptor.dimension,
                        descriptor.stream,
                        source.GetVertexAttributeOffset(descriptor.attribute))));
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatch] source={0}, readable={1}, vertices={2}, " +
                    "submeshes={3}, layout=[{4}]",
                    assetPath,
                    source.isReadable,
                    vertexCount,
                    subMeshCount,
                    layout);
            }

            private void ValidateTransformAttribute(VertexAttribute attribute)
            {
                VertexAttributeDescriptor descriptor = descriptorByAttribute[attribute];
                if (descriptor.dimension < 3 || descriptor.dimension > 4 ||
                    (descriptor.format != VertexAttributeFormat.Float32 &&
                     descriptor.format != VertexAttributeFormat.Float16))
                {
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot safely transform {0} stored as {1}x{2}.",
                        attribute,
                        descriptor.format,
                        descriptor.dimension));
                }
            }

            private void CaptureSubmeshIndices<T>(Mesh.MeshData meshData, NativeArray<T> indexData)
                where T : struct, IConvertible
            {
                for (int submesh = 0; submesh < subMeshCount; submesh++)
                {
                    SubMeshDescriptor descriptor = meshData.GetSubMesh(submesh);
                    int[] indices = new int[descriptor.indexCount];
                    for (int index = 0; index < descriptor.indexCount; index++)
                    {
                        long rawIndex = Convert.ToInt64(
                            indexData[descriptor.indexStart + index],
                            CultureInfo.InvariantCulture);
                        indices[index] = checked((int)rawIndex + descriptor.baseVertex);
                    }

                    indicesBySubmesh.Add(submesh, indices);
                    topologyBySubmesh.Add(submesh, descriptor.topology);
                }
            }
        }

        private static int GetFormatSize(VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                    return 4;
                case VertexAttributeFormat.Float16:
                    return 2;
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.SNorm8:
                case VertexAttributeFormat.UInt8:
                case VertexAttributeFormat.SInt8:
                    return 1;
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.SNorm16:
                case VertexAttributeFormat.UInt16:
                case VertexAttributeFormat.SInt16:
                    return 2;
                case VertexAttributeFormat.UInt32:
                case VertexAttributeFormat.SInt32:
                    return 4;
                default:
                    throw new NotSupportedException("Vertex format size is not implemented for " + format + ".");
            }
        }

        private static float ReadFloat(byte[] bytes, int offset, VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                {
                    uint bits =
                        (uint)bytes[offset] |
                        ((uint)bytes[offset + 1] << 8) |
                        ((uint)bytes[offset + 2] << 16) |
                        ((uint)bytes[offset + 3] << 24);
                    var union = new FloatUIntUnion { uintValue = bits };
                    return union.floatValue;
                }
                case VertexAttributeFormat.Float16:
                {
                    ushort bits = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
                    return HalfToFloat(bits);
                }
                default:
                    throw new NotSupportedException("Float conversion is not implemented for " + format + ".");
            }
        }

        private static void WriteFloat(
            byte[] bytes,
            int offset,
            VertexAttributeFormat format,
            float value)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                {
                    var union = new FloatUIntUnion { floatValue = value };
                    uint bits = union.uintValue;
                    bytes[offset] = (byte)bits;
                    bytes[offset + 1] = (byte)(bits >> 8);
                    bytes[offset + 2] = (byte)(bits >> 16);
                    bytes[offset + 3] = (byte)(bits >> 24);
                    break;
                }
                case VertexAttributeFormat.Float16:
                {
                    ushort bits = FloatToHalf(value);
                    bytes[offset] = (byte)bits;
                    bytes[offset + 1] = (byte)(bits >> 8);
                    break;
                }
                default:
                    throw new NotSupportedException("Float conversion is not implemented for " + format + ".");
            }
        }

        // IEEE-754 binary16 conversion kept local so the migration does not add
        // a Unity.Mathematics/package dependency.
        private static float HalfToFloat(ushort value)
        {
            uint sign = (uint)(value & 0x8000) << 16;
            uint exponent = (uint)(value >> 10) & 0x1f;
            uint mantissa = (uint)value & 0x3ff;
            uint result;

            if (exponent == 0)
            {
                if (mantissa == 0)
                {
                    result = sign;
                }
                else
                {
                    exponent = 1;
                    while ((mantissa & 0x400) == 0)
                    {
                        mantissa <<= 1;
                        exponent--;
                    }

                    mantissa &= 0x3ff;
                    uint floatExponent = exponent + (127 - 15);
                    result = sign | (floatExponent << 23) | (mantissa << 13);
                }
            }
            else if (exponent == 0x1f)
            {
                result = sign | 0x7f800000 | (mantissa << 13);
            }
            else
            {
                result = sign | ((exponent + (127 - 15)) << 23) | (mantissa << 13);
            }

            return new FloatUIntUnion { uintValue = result }.floatValue;
        }

        private static ushort FloatToHalf(float value)
        {
            uint bits = new FloatUIntUnion { floatValue = value }.uintValue;
            uint sign = (bits >> 16) & 0x8000;
            int exponent = (int)((bits >> 23) & 0xff) - 127 + 15;
            uint mantissa = bits & 0x7fffff;

            if (((bits >> 23) & 0xff) == 0xff)
            {
                return (ushort)(sign | (mantissa == 0 ? 0x7c00u : 0x7e00u));
            }

            if (exponent <= 0)
            {
                if (exponent < -10)
                {
                    return (ushort)sign;
                }

                mantissa = (mantissa | 0x800000) >> (1 - exponent);
                // Round to nearest, ties to even at the binary16 cut.
                uint rounded = mantissa + 0x0fff + ((mantissa >> 13) & 1);
                return (ushort)(sign | (rounded >> 13));
            }

            if (exponent >= 31)
            {
                return (ushort)(sign | 0x7c00);
            }

            mantissa += 0x0fff + ((mantissa >> 13) & 1);
            if ((mantissa & 0x800000) != 0)
            {
                mantissa = 0;
                exponent++;
                if (exponent >= 31)
                {
                    return (ushort)(sign | 0x7c00);
                }
            }

            return (ushort)(sign | ((uint)exponent << 10) | (mantissa >> 13));
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatUIntUnion
        {
            [FieldOffset(0)] public float floatValue;
            [FieldOffset(0)] public uint uintValue;
        }

        private sealed class LegacyStaticBatchDataException : Exception
        {
            public LegacyStaticBatchDataException(string message)
                : base(message)
            {
            }
        }
    }
}
#endif
