#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace RacingConversion.Editor
{
    /// <summary>
    /// Strict multi-scene migration for the gameplay scenes reached by the city,
    /// airport, and off-road smoke tests. It deliberately does not include Trial,
    /// City_2, Crashable, Radar, Store, or Workshop scenes.
    /// </summary>
    internal static class GameplayLegacyStaticBatchMigration
    {
        private const string GeneratedBase =
            "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches";
        private const string BackupBase =
            "MigrationBackups/GameplayLegacyStaticBatches";

        private static readonly SceneSpec ForestScene = new SceneSpec(
            "Extreme_2Game_Forest",
            "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Forest.unity",
            925,
            1259,
            4);

        private static readonly SceneSpec[] SupportedScenes =
        {
            new SceneSpec(
                "Common_2Game",
                "Assets/Scenes/Common/2Game.unity",
                270,
                364,
                1),
            new SceneSpec(
                "Extreme_2Game_City",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_City.unity",
                437,
                461,
                3),
            new SceneSpec(
                "Extreme_2Game_Airport",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport.unity",
                699,
                764,
                2),
            new SceneSpec(
                "Extreme_2Game_Airport_Zone0",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone0.unity",
                160,
                172,
                1),
            new SceneSpec(
                "Extreme_2Game_Airport_Zone1",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone1.unity",
                13,
                21,
                1),
            new SceneSpec(
                "Extreme_2Game_Airport_Zone2",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone2.unity",
                20,
                20,
                1),
            ForestScene,
            new SceneSpec(
                "Extreme_2Game_Zone0",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone0.unity",
                207,
                237,
                1),
            new SceneSpec(
                "Extreme_2Game_Zone1",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone1.unity",
                76,
                88,
                1),
            new SceneSpec(
                "Extreme_2Game_Zone2",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone2.unity",
                75,
                101,
                1),
            new SceneSpec(
                "Extreme_2Game_Zone3",
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone3.unity",
                41,
                57,
                1)
        };

        private static readonly Regex DocumentHeader = new Regex(
            @"^--- !u!(?<classId>-?\d+) &(?<fileId>-?\d+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex ObjectReference = new Regex(
            @"\{fileID:\s*(?<fileId>-?\d+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex ExternalMeshReference = new Regex(
            @"^\{fileID:\s*(?<fileId>-?\d+),\s*guid:\s*(?<guid>[0-9a-fA-F]{32}),\s*type:\s*(?<type>\d+)\}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex InlineVector4 = new Regex(
            @"^\{x:\s*(?<x>[^,]+),\s*y:\s*(?<y>[^,]+),\s*z:\s*(?<z>[^,]+),\s*w:\s*(?<w>[^}]+)\}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [MenuItem("Racing Conversion/Legacy Static Batches/Analyze Gameplay Scenes")]
        public static void AnalyzeGameplayScenes()
        {
            AnalyzeGameplayScenesFromCommandLine();
        }

        [MenuItem("Racing Conversion/Legacy Static Batches/Migrate Gameplay Scenes")]
        public static void MigrateGameplayScenes()
        {
            MigrateGameplayScenesFromCommandLine();
        }

        [MenuItem("Racing Conversion/Legacy Static Batches/Migrate Off-Road Scene")]
        public static void MigrateForestSceneFromCommandLine()
        {
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[GameplayLegacyStaticBatch] Off-road migration cancelled.");
                return;
            }

            // Keep the previously migrated City and Airport scene files untouched.
            // MigrateScene backs up the raw YAML before Unity can discard its
            // legacy subset indices, then reopens and validates every renderer.
            SceneMigrationResult result = MigrateScene(ForestScene);
            if (result.validatedCount != 925)
            {
                throw new GameplayMigrationDataException(
                    "Off-road migration did not validate all 925 renderer records.");
            }
        }

        public static void AnalyzeGameplayScenesFromCommandLine()
        {
            int totalRenderers = 0;
            int totalSubmeshes = 0;
            int totalCombinedMeshes = 0;

            foreach (SceneSpec spec in SupportedScenes)
            {
                string rawYaml = File.ReadAllText(ToAbsoluteProjectPath(spec.scenePath));
                List<RendererSubsetRecord> records = ParseRawScene(rawYaml);
                ValidateInventory(spec, records);

                int submeshCount = records.Sum(record => record.submeshes.Length);
                int combinedMeshCount = records
                    .Select(record => record.sourceMeshGuid + ":" + record.sourceMeshFileId)
                    .Distinct()
                    .Count();
                totalRenderers += records.Count;
                totalSubmeshes += submeshCount;
                totalCombinedMeshes += combinedMeshCount;

                Debug.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[GameplayLegacyStaticBatch] analysis scene={0}, renderers={1}, " +
                    "selectedSubmeshes={2}, combinedMeshes={3}, meshGuids=[{4}]",
                    spec.scenePath,
                    records.Count,
                    submeshCount,
                    combinedMeshCount,
                    string.Join(",", records.Select(record => record.sourceMeshGuid).Distinct())));
            }

            if (totalRenderers != 2923 || totalSubmeshes != 3544 || totalCombinedMeshes != 17)
            {
                throw new GameplayMigrationDataException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected aggregate inventory: renderers={0}, submeshes={1}, meshes={2}.",
                    totalRenderers,
                    totalSubmeshes,
                    totalCombinedMeshes));
            }

            Debug.Log(string.Format(
                CultureInfo.InvariantCulture,
                "[GameplayLegacyStaticBatch] aggregate analysis validated: scenes={0}, " +
                "renderers={1}, selectedSubmeshes={2}, combinedMeshes={3}.",
                SupportedScenes.Length,
                totalRenderers,
                totalSubmeshes,
                totalCombinedMeshes));
        }

        public static void MigrateGameplayScenesFromCommandLine()
        {
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[GameplayLegacyStaticBatch] Migration cancelled.");
                return;
            }

            int totalChanged = 0;
            int totalValidated = 0;
            foreach (SceneSpec spec in SupportedScenes)
            {
                SceneMigrationResult result = MigrateScene(spec);
                totalChanged += result.changedCount;
                totalValidated += result.validatedCount;
            }

            if (totalValidated != 2923)
            {
                throw new GameplayMigrationDataException(
                    "Post-migration validation did not cover all 2,923 renderer records.");
            }

            Debug.Log(string.Format(
                CultureInfo.InvariantCulture,
                "[GameplayLegacyStaticBatch] all supported gameplay scenes complete: " +
                "scenes={0}, changed={1}, validated={2}; no player build was run.",
                SupportedScenes.Length,
                totalChanged,
                totalValidated));
        }

        private static SceneMigrationResult MigrateScene(SceneSpec spec)
        {
            string absoluteScenePath = ToAbsoluteProjectPath(spec.scenePath);
            if (!File.Exists(absoluteScenePath))
            {
                throw new FileNotFoundException("Gameplay scene is missing.", absoluteScenePath);
            }

            byte[] rawSceneBytes = File.ReadAllBytes(absoluteScenePath);
            string currentHash = ComputeSha256(rawSceneBytes);
            string sceneGuidBefore = AssetDatabase.AssetPathToGUID(spec.scenePath);
            if (string.IsNullOrEmpty(sceneGuidBefore))
            {
                throw new GameplayMigrationDataException(
                    "Scene has no AssetDatabase GUID: " + spec.scenePath);
            }

            string backupPath = EnsureRawSceneBackup(spec, absoluteScenePath, currentHash);
            List<RendererSubsetRecord> rawRecords = ParseRawScene(
                Encoding.UTF8.GetString(rawSceneBytes));
            GameplaySubsetManifest manifest = MergeAndPersistManifest(
                spec,
                rawRecords,
                sceneGuidBefore,
                currentHash,
                backupPath);
            ValidateInventory(spec, manifest.renderers);

            Scene scene = EditorSceneManager.OpenScene(spec.scenePath, OpenSceneMode.Single);
            Dictionary<long, MeshRenderer> renderersByFileId = IndexSceneRenderers(scene);
            var extractor = new LegacyStaticBatchMigration.ReusableMeshExtractor();
            var staged = new List<StagedMigration>(manifest.renderers.Count);

            try
            {
                EnsureAssetFolder(spec.generatedMeshFolder);

                foreach (RendererSubsetRecord record in manifest.renderers
                             .OrderBy(value => value.rendererFileId))
                {
                    MeshRenderer renderer = ResolveRenderer(record, renderersByFileId);
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null)
                    {
                        throw new GameplayMigrationDataException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} in {1} has no MeshFilter.",
                            record.rendererFileId,
                            spec.scenePath));
                    }

                    long actualFilterFileId = GetRequiredLocalId(filter, "MeshFilter");
                    if (actualFilterFileId != record.meshFilterFileId)
                    {
                        throw new GameplayMigrationDataException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} MeshFilter local-ID mismatch: manifest={1}, scene={2}.",
                            record.rendererFileId,
                            record.meshFilterFileId,
                            actualFilterFileId));
                    }

                    string generatedPath = GetGeneratedMeshPath(spec, record);
                    string currentMeshPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                    if (string.Equals(currentMeshPath, generatedPath, StringComparison.Ordinal))
                    {
                        ValidateGeneratedMesh(renderer, filter.sharedMesh, record, generatedPath);
                        staged.Add(StagedMigration.AlreadyComplete(record, renderer, filter, generatedPath));
                        continue;
                    }

                    Mesh sourceMesh = filter.sharedMesh;
                    if (sourceMesh == null)
                    {
                        throw new GameplayMigrationDataException(
                            "Missing source mesh for renderer " + record.rendererFileId);
                    }

                    string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
                    string sourceGuid;
                    long sourceMeshFileId;
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                            sourceMesh,
                            out sourceGuid,
                            out sourceMeshFileId))
                    {
                        throw new GameplayMigrationDataException(
                            "Could not resolve source mesh GUID/fileID for " + sourcePath);
                    }

                    if (!string.Equals(
                            sourceGuid,
                            record.sourceMeshGuid,
                            StringComparison.OrdinalIgnoreCase) ||
                        sourceMeshFileId != record.sourceMeshFileId)
                    {
                        throw new GameplayMigrationDataException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} source mesh mismatch: manifest={1}/{2}, scene={3}/{4} ({5}).",
                            record.rendererFileId,
                            record.sourceMeshGuid,
                            record.sourceMeshFileId,
                            sourceGuid,
                            sourceMeshFileId,
                            sourcePath));
                    }

                    if (renderer.sharedMaterials.Length != record.submeshes.Length)
                    {
                        throw new GameplayMigrationDataException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Renderer {0} ({1}) has {2} materials for {3} selected submeshes.",
                            record.rendererFileId,
                            record.gameObjectName,
                            renderer.sharedMaterials.Length,
                            record.submeshes.Length));
                    }

                    LegacyStaticBatchMigration.ReusableExtractionResult extraction =
                        extractor.Extract(
                            sourceMesh,
                            sourcePath,
                            renderer.transform,
                            record.rendererFileId,
                            record.gameObjectName,
                            record.submeshes,
                            spec.key);
                    Mesh generatedMesh = PersistGeneratedMesh(extraction.mesh, generatedPath);
                    string generatedGuid = AssetDatabase.AssetPathToGUID(generatedPath);
                    if (string.IsNullOrEmpty(generatedGuid))
                    {
                        throw new GameplayMigrationDataException(
                            "Generated mesh has no GUID: " + generatedPath);
                    }

                    record.generatedMeshPath = generatedPath;
                    record.generatedMeshGuid = generatedGuid;
                    record.generatedVertexCount = extraction.vertexCount;
                    record.generatedIndexCount = extraction.indexCount;
                    record.sourceWorldBoundsCenter = extraction.sourceWorldBounds.center;
                    record.sourceWorldBoundsExtents = extraction.sourceWorldBounds.extents;
                    record.generatedLocalBoundsCenter = extraction.outputLocalBounds.center;
                    record.generatedLocalBoundsExtents = extraction.outputLocalBounds.extents;
                    record.maxPositionRoundTripError = extraction.maxRoundTripError;
                    record.materialCount = renderer.sharedMaterials.Length;
                    record.lodReferenceCount = CountLodReferences(scene, renderer);

                    staged.Add(new StagedMigration(
                        record,
                        renderer,
                        filter,
                        generatedMesh,
                        generatedPath,
                        extraction));
                }

                AssetDatabase.SaveAssets();
                int changedCount = 0;
                foreach (StagedMigration migration in staged)
                {
                    if (migration.alreadyComplete)
                    {
                        continue;
                    }

                    RendererState before = RendererState.Capture(scene, migration.renderer);
                    migration.filter.sharedMesh = migration.generatedMesh;
                    ClearLegacyStaticBatchState(migration.renderer);
                    StaticEditorFlags flagsAfter =
                        before.staticFlags & ~StaticEditorFlags.BatchingStatic;
                    GameObjectUtility.SetStaticEditorFlags(
                        migration.renderer.gameObject,
                        flagsAfter);

                    // These values are validated here and preserved in the
                    // manifest. Unity 6 strips them from the saved scene, so the
                    // build-scene processor reapplies them to the temporary copy.
                    migration.renderer.lightmapIndex = migration.record.lightmapIndex;
                    migration.renderer.lightmapScaleOffset =
                        migration.record.lightmapScaleOffset;

                    ValidateRendererStateAfterAssignment(
                        scene,
                        migration.renderer,
                        before,
                        flagsAfter,
                        migration.record);
                    EditorUtility.SetDirty(migration.filter);
                    EditorUtility.SetDirty(migration.renderer);
                    EditorUtility.SetDirty(migration.renderer.gameObject);
                    changedCount++;

                    Debug.Log(migration.Describe(spec.scenePath, before.staticFlags, flagsAfter));
                }

                if (changedCount > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    AssetDatabase.SaveAssets();
                    if (!EditorSceneManager.SaveScene(scene, spec.scenePath, false))
                    {
                        throw new IOException("Unity failed to save " + spec.scenePath);
                    }
                    AssetDatabase.SaveAssets();
                }

                PersistManifest(spec, manifest);
                string sceneGuidAfter = AssetDatabase.AssetPathToGUID(spec.scenePath);
                if (!string.Equals(sceneGuidBefore, sceneGuidAfter, StringComparison.Ordinal))
                {
                    throw new GameplayMigrationDataException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Scene GUID changed for {0}: {1} -> {2}.",
                        spec.scenePath,
                        sceneGuidBefore,
                        sceneGuidAfter));
                }

                int validatedCount = ReopenAndValidate(spec, manifest, sceneGuidBefore);
                Debug.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[GameplayLegacyStaticBatch] scene complete: scene={0}, requested={1}, " +
                    "changed={2}, alreadyComplete={3}, validated={4}, sceneGuid={5}, " +
                    "rawBackup={6}, manifest={7}",
                    spec.scenePath,
                    manifest.renderers.Count,
                    changedCount,
                    staged.Count - changedCount,
                    validatedCount,
                    sceneGuidAfter,
                    manifest.originalRawBackupPath,
                    spec.manifestPath));

                return new SceneMigrationResult(changedCount, validatedCount);
            }
            catch
            {
                Debug.LogError(string.Format(
                    CultureInfo.InvariantCulture,
                    "[GameplayLegacyStaticBatch] Migration failed for {0}. Raw scene backup: {1}",
                    spec.scenePath,
                    manifest.originalRawBackupPath));
                throw;
            }
        }

        private static int ReopenAndValidate(
            SceneSpec spec,
            GameplaySubsetManifest manifest,
            string expectedSceneGuid)
        {
            Scene scene = EditorSceneManager.OpenScene(spec.scenePath, OpenSceneMode.Single);
            if (!string.Equals(
                    AssetDatabase.AssetPathToGUID(spec.scenePath),
                    expectedSceneGuid,
                    StringComparison.Ordinal))
            {
                throw new GameplayMigrationDataException(
                    "Scene GUID mismatch while reopening " + spec.scenePath);
            }

            Dictionary<long, MeshRenderer> renderers = IndexSceneRenderers(scene);
            int validated = 0;
            foreach (RendererSubsetRecord record in manifest.renderers)
            {
                MeshRenderer renderer = ResolveRenderer(record, renderers);
                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (filter == null)
                {
                    throw new GameplayMigrationDataException(
                        "Reopened renderer has no MeshFilter: " + record.rendererFileId);
                }

                string expectedPath = GetGeneratedMeshPath(spec, record);
                string actualPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                if (!string.Equals(expectedPath, actualPath, StringComparison.Ordinal))
                {
                    throw new GameplayMigrationDataException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Reopened mesh path mismatch for renderer {0}: expected={1}, actual={2}.",
                        record.rendererFileId,
                        expectedPath,
                        actualPath));
                }

                ValidateGeneratedMesh(renderer, filter.sharedMesh, record, expectedPath);
                if ((GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) &
                     StaticEditorFlags.BatchingStatic) != 0)
                {
                    throw new GameplayMigrationDataException(
                        "BatchingStatic remained set for renderer " + record.rendererFileId);
                }

                if (CountLodReferences(scene, renderer) != record.lodReferenceCount)
                {
                    throw new GameplayMigrationDataException(
                        "LOD reference count changed for renderer " + record.rendererFileId);
                }

                validated++;
            }

            return validated;
        }

        private static void ValidateGeneratedMesh(
            MeshRenderer renderer,
            Mesh mesh,
            RendererSubsetRecord record,
            string path)
        {
            if (mesh == null)
            {
                throw new GameplayMigrationDataException("Generated mesh is null: " + path);
            }

            if (mesh.subMeshCount != record.submeshes.Length ||
                renderer.sharedMaterials.Length != record.submeshes.Length)
            {
                throw new GameplayMigrationDataException(
                    "Generated mesh/material submesh mismatch: " + path);
            }

            int indexCount = GetMeshIndexCount(mesh);
            if (record.generatedVertexCount > 0 && mesh.vertexCount != record.generatedVertexCount)
            {
                throw new GameplayMigrationDataException(
                    "Generated vertex count changed: " + path);
            }

            if (record.generatedIndexCount > 0 && indexCount != record.generatedIndexCount)
            {
                throw new GameplayMigrationDataException(
                    "Generated index count changed: " + path);
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(record.generatedMeshGuid) &&
                !string.Equals(guid, record.generatedMeshGuid, StringComparison.Ordinal))
            {
                throw new GameplayMigrationDataException(
                    "Generated mesh GUID changed: " + path);
            }

            record.generatedMeshPath = path;
            record.generatedMeshGuid = guid;
            record.generatedVertexCount = mesh.vertexCount;
            record.generatedIndexCount = indexCount;
            record.materialCount = renderer.sharedMaterials.Length;
        }

        private static int GetMeshIndexCount(Mesh mesh)
        {
            long count = 0;
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                count += mesh.GetIndexCount(submesh);
            }

            return checked((int)count);
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
            SerializedProperty subset = serializedRenderer.FindProperty("m_SubsetIndices");
            if (subset != null && subset.isArray)
            {
                subset.arraySize = 0;
            }

            SerializedProperty batchRoot = serializedRenderer.FindProperty("m_StaticBatchRoot");
            if (batchRoot != null &&
                batchRoot.propertyType == SerializedPropertyType.ObjectReference)
            {
                batchRoot.objectReferenceValue = null;
            }

            SerializedProperty batchInfo = serializedRenderer.FindProperty("m_StaticBatchInfo");
            if (batchInfo != null)
            {
                SerializedProperty first = batchInfo.FindPropertyRelative("firstSubMesh");
                SerializedProperty count = batchInfo.FindPropertyRelative("subMeshCount");
                if (first != null)
                {
                    first.intValue = 0;
                }

                if (count != null)
                {
                    count.intValue = 0;
                }
            }

            serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ValidateRendererStateAfterAssignment(
            Scene scene,
            MeshRenderer renderer,
            RendererState before,
            StaticEditorFlags expectedFlags,
            RendererSubsetRecord record)
        {
            if (!before.materials.SequenceEqual(renderer.sharedMaterials))
            {
                throw new GameplayMigrationDataException(
                    "Materials changed for renderer " + record.rendererFileId);
            }

            if (CountLodReferences(scene, renderer) != before.lodReferenceCount)
            {
                throw new GameplayMigrationDataException(
                    "LOD references changed for renderer " + record.rendererFileId);
            }

            if (renderer.enabled != before.enabled ||
                renderer.shadowCastingMode != before.shadowCastingMode ||
                renderer.receiveShadows != before.receiveShadows ||
                renderer.lightProbeUsage != before.lightProbeUsage ||
                renderer.reflectionProbeUsage != before.reflectionProbeUsage ||
                renderer.sortingLayerID != before.sortingLayerId ||
                renderer.sortingOrder != before.sortingOrder ||
                GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) != expectedFlags)
            {
                throw new GameplayMigrationDataException(
                    "Renderer state changed unexpectedly for " + record.rendererFileId);
            }

            if (renderer.lightmapIndex != record.lightmapIndex ||
                renderer.lightmapScaleOffset != record.lightmapScaleOffset)
            {
                throw new GameplayMigrationDataException(
                    "Unity rejected recovered lightmap metadata for " + record.rendererFileId);
            }
        }

        private static Dictionary<long, MeshRenderer> IndexSceneRenderers(Scene scene)
        {
            var result = new Dictionary<long, MeshRenderer>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    long localId = GetLocalId(renderer);
                    if (localId == 0)
                    {
                        continue;
                    }

                    if (result.ContainsKey(localId))
                    {
                        throw new GameplayMigrationDataException(
                            "Duplicate scene renderer local ID " + localId);
                    }

                    result.Add(localId, renderer);
                }
            }

            return result;
        }

        private static MeshRenderer ResolveRenderer(
            RendererSubsetRecord record,
            Dictionary<long, MeshRenderer> renderers)
        {
            MeshRenderer renderer;
            if (!renderers.TryGetValue(record.rendererFileId, out renderer))
            {
                throw new GameplayMigrationDataException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Could not resolve renderer local ID {0} ({1}); no name fallback is permitted.",
                    record.rendererFileId,
                    record.gameObjectName));
            }

            long gameObjectId = GetRequiredLocalId(renderer.gameObject, "GameObject");
            if (gameObjectId != record.gameObjectFileId)
            {
                throw new GameplayMigrationDataException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Renderer {0} GameObject local-ID mismatch: manifest={1}, scene={2}.",
                    record.rendererFileId,
                    record.gameObjectFileId,
                    gameObjectId));
            }

            return renderer;
        }

        private static long GetRequiredLocalId(UnityEngine.Object target, string kind)
        {
            long localId = GetLocalId(target);
            if (localId == 0)
            {
                throw new GameplayMigrationDataException(
                    "Could not resolve persistent local ID for " + kind + " " + target.name);
            }

            return localId;
        }

        private static long GetLocalId(UnityEngine.Object target)
        {
            string guid;
            long localId;
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out guid, out localId) &&
                localId != 0)
            {
                return localId;
            }

            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(target);
            return unchecked((long)globalId.targetObjectId);
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

        private static GameplaySubsetManifest MergeAndPersistManifest(
            SceneSpec spec,
            List<RendererSubsetRecord> rawRecords,
            string sceneGuid,
            string sourceHash,
            string currentBackupPath)
        {
            GameplaySubsetManifest existing = null;
            string absoluteManifestPath = ToAbsoluteProjectPath(spec.manifestPath);
            if (File.Exists(absoluteManifestPath))
            {
                existing = JsonUtility.FromJson<GameplaySubsetManifest>(
                    File.ReadAllText(absoluteManifestPath));
            }

            var merged = new Dictionary<long, RendererSubsetRecord>();
            if (existing != null)
            {
                if (!string.Equals(existing.scenePath, spec.scenePath, StringComparison.Ordinal) ||
                    !string.Equals(existing.sceneGuid, sceneGuid, StringComparison.Ordinal))
                {
                    throw new GameplayMigrationDataException(
                        "Existing manifest identity mismatch: " + spec.manifestPath);
                }

                if (existing.renderers != null)
                {
                    foreach (RendererSubsetRecord record in existing.renderers)
                    {
                        merged[record.rendererFileId] = record;
                    }
                }

                if (!string.IsNullOrEmpty(existing.originalRawBackupPath) &&
                    File.Exists(existing.originalRawBackupPath))
                {
                    foreach (RendererSubsetRecord record in ParseRawScene(
                                 File.ReadAllText(existing.originalRawBackupPath)))
                    {
                        PreserveGeneratedFields(record, merged);
                        merged[record.rendererFileId] = record;
                    }
                }
            }

            foreach (RendererSubsetRecord record in rawRecords)
            {
                PreserveGeneratedFields(record, merged);
                merged[record.rendererFileId] = record;
            }

            var manifest = new GameplaySubsetManifest
            {
                sceneKey = spec.key,
                scenePath = spec.scenePath,
                sceneGuid = sceneGuid,
                originalSourceSha256 = existing != null &&
                                       !string.IsNullOrEmpty(existing.originalSourceSha256)
                    ? existing.originalSourceSha256
                    : sourceHash,
                originalRawBackupPath = existing != null &&
                                        !string.IsNullOrEmpty(existing.originalRawBackupPath)
                    ? existing.originalRawBackupPath
                    : currentBackupPath,
                expectedRendererCount = spec.expectedRendererCount,
                expectedSelectedSubmeshCount = spec.expectedSelectedSubmeshCount,
                expectedCombinedMeshCount = spec.expectedCombinedMeshCount,
                renderers = merged.Values.OrderBy(record => record.rendererFileId).ToList()
            };
            ValidateInventory(spec, manifest.renderers);
            PersistManifest(spec, manifest);
            return manifest;
        }

        private static void PreserveGeneratedFields(
            RendererSubsetRecord replacement,
            Dictionary<long, RendererSubsetRecord> existingRecords)
        {
            RendererSubsetRecord existing;
            if (!existingRecords.TryGetValue(replacement.rendererFileId, out existing))
            {
                return;
            }

            replacement.generatedMeshPath = existing.generatedMeshPath;
            replacement.generatedMeshGuid = existing.generatedMeshGuid;
            replacement.generatedVertexCount = existing.generatedVertexCount;
            replacement.generatedIndexCount = existing.generatedIndexCount;
            replacement.materialCount = existing.materialCount;
            replacement.lodReferenceCount = existing.lodReferenceCount;
            replacement.sourceWorldBoundsCenter = existing.sourceWorldBoundsCenter;
            replacement.sourceWorldBoundsExtents = existing.sourceWorldBoundsExtents;
            replacement.generatedLocalBoundsCenter = existing.generatedLocalBoundsCenter;
            replacement.generatedLocalBoundsExtents = existing.generatedLocalBoundsExtents;
            replacement.maxPositionRoundTripError = existing.maxPositionRoundTripError;
        }

        private static void PersistManifest(SceneSpec spec, GameplaySubsetManifest manifest)
        {
            EnsureAssetFolder(spec.generatedRoot);
            File.WriteAllText(
                ToAbsoluteProjectPath(spec.manifestPath),
                JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(
                spec.manifestPath,
                ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ValidateInventory(
            SceneSpec spec,
            IList<RendererSubsetRecord> records)
        {
            int selectedSubmeshes = records.Sum(record => record.submeshes.Length);
            int combinedMeshes = records
                .Select(record => record.sourceMeshGuid + ":" + record.sourceMeshFileId)
                .Distinct()
                .Count();
            if (records.Count != spec.expectedRendererCount ||
                selectedSubmeshes != spec.expectedSelectedSubmeshCount ||
                combinedMeshes != spec.expectedCombinedMeshCount)
            {
                throw new GameplayMigrationDataException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Inventory mismatch for {0}: renderers={1}/{2}, submeshes={3}/{4}, " +
                    "combinedMeshes={5}/{6}.",
                    spec.scenePath,
                    records.Count,
                    spec.expectedRendererCount,
                    selectedSubmeshes,
                    spec.expectedSelectedSubmeshCount,
                    combinedMeshes,
                    spec.expectedCombinedMeshCount));
            }

            if (records.GroupBy(record => record.rendererFileId).Any(group => group.Count() != 1) ||
                records.Any(record =>
                    record.rendererFileId == 0 ||
                    record.gameObjectFileId == 0 ||
                    record.meshFilterFileId == 0 ||
                    record.sourceMeshFileId == 0 ||
                    string.IsNullOrEmpty(record.sourceMeshGuid)))
            {
                throw new GameplayMigrationDataException(
                    "Duplicate or incomplete renderer records in " + spec.scenePath);
            }
        }

        private static List<RendererSubsetRecord> ParseRawScene(string rawYaml)
        {
            var names = new Dictionary<long, string>();
            var filters = new Dictionary<long, MeshFilterRecord>();
            var pendingRenderers = new List<PendingRendererRecord>();

            int currentClassId = 0;
            long currentFileId = 0;
            long currentGameObjectId = 0;
            string currentName = null;
            string currentSubsetHex = null;
            int currentSourceLightmapIndex = -1;
            Vector4 currentLightmapScaleOffset = new Vector4(1, 1, 0, 0);
            bool hasLightmapIndex = false;
            bool hasLightmapScaleOffset = false;
            long currentMeshFileId = 0;
            string currentMeshGuid = null;

            Action flush = () =>
            {
                if (currentClassId == 1 && currentFileId != 0 && currentName != null)
                {
                    names[currentFileId] = currentName;
                }
                else if (currentClassId == 33 &&
                         currentFileId != 0 &&
                         currentGameObjectId != 0 &&
                         currentMeshFileId != 0 &&
                         !string.IsNullOrEmpty(currentMeshGuid))
                {
                    if (filters.ContainsKey(currentGameObjectId))
                    {
                        throw new GameplayMigrationDataException(
                            "Multiple external MeshFilters found on GameObject " +
                            currentGameObjectId);
                    }

                    filters.Add(currentGameObjectId, new MeshFilterRecord
                    {
                        meshFilterFileId = currentFileId,
                        gameObjectFileId = currentGameObjectId,
                        sourceMeshFileId = currentMeshFileId,
                        sourceMeshGuid = currentMeshGuid.ToLowerInvariant()
                    });
                }
                else if (currentClassId == 23 &&
                         currentFileId != 0 &&
                         currentGameObjectId != 0 &&
                         !string.IsNullOrWhiteSpace(currentSubsetHex))
                {
                    if (!hasLightmapIndex || !hasLightmapScaleOffset)
                    {
                        throw new GameplayMigrationDataException(
                            "Incomplete lightmap metadata for renderer " + currentFileId);
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
                        flush();
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
                        hasLightmapIndex = false;
                        hasLightmapScaleOffset = false;
                        currentMeshFileId = 0;
                        currentMeshGuid = null;
                        continue;
                    }

                    if (currentClassId == 1 &&
                        line.StartsWith("  m_Name: ", StringComparison.Ordinal))
                    {
                        currentName = UnquoteYamlScalar(line.Substring("  m_Name: ".Length));
                    }
                    else if ((currentClassId == 23 || currentClassId == 33) &&
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
                    else if (currentClassId == 33 &&
                             line.StartsWith("  m_Mesh: ", StringComparison.Ordinal))
                    {
                        Match reference = ExternalMeshReference.Match(
                            line.Substring("  m_Mesh: ".Length).Trim());
                        if (reference.Success)
                        {
                            currentMeshFileId = long.Parse(
                                reference.Groups["fileId"].Value,
                                CultureInfo.InvariantCulture);
                            currentMeshGuid = reference.Groups["guid"].Value;
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
                        hasLightmapIndex = true;
                    }
                    else if (currentClassId == 23 &&
                             line.StartsWith("  m_LightmapTilingOffset:", StringComparison.Ordinal))
                    {
                        currentLightmapScaleOffset = ParseInlineVector4(
                            line.Substring("  m_LightmapTilingOffset:".Length).Trim());
                        hasLightmapScaleOffset = true;
                    }
                }
            }

            flush();

            var records = new List<RendererSubsetRecord>(pendingRenderers.Count);
            foreach (PendingRendererRecord pending in pendingRenderers)
            {
                MeshFilterRecord filter;
                if (!filters.TryGetValue(pending.gameObjectFileId, out filter))
                {
                    throw new GameplayMigrationDataException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Renderer {0} GameObject {1} has no unambiguous external MeshFilter reference.",
                        pending.rendererFileId,
                        pending.gameObjectFileId));
                }

                string name;
                if (!names.TryGetValue(pending.gameObjectFileId, out name))
                {
                    throw new GameplayMigrationDataException(
                        "Missing GameObject name for " + pending.gameObjectFileId);
                }

                records.Add(new RendererSubsetRecord
                {
                    rendererFileId = pending.rendererFileId,
                    gameObjectFileId = pending.gameObjectFileId,
                    meshFilterFileId = filter.meshFilterFileId,
                    gameObjectName = name,
                    subsetHexLittleEndian = pending.subsetHex,
                    submeshes = DecodeLittleEndianInt32Array(pending.subsetHex),
                    sourceMeshGuid = filter.sourceMeshGuid,
                    sourceMeshFileId = filter.sourceMeshFileId,
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
            if (string.IsNullOrEmpty(hex) || hex.Length % 8 != 0)
            {
                throw new GameplayMigrationDataException(
                    "Invalid m_SubsetIndices byte string: " + hex);
            }

            var result = new int[hex.Length / 8];
            for (int index = 0; index < result.Length; index++)
            {
                int offset = index * 8;
                uint value =
                    ParseHexByte(hex, offset) |
                    (ParseHexByte(hex, offset + 2) << 8) |
                    (ParseHexByte(hex, offset + 4) << 16) |
                    (ParseHexByte(hex, offset + 6) << 24);
                result[index] = unchecked((int)value);
            }

            return result;
        }

        private static uint ParseHexByte(string value, int offset)
        {
            return uint.Parse(
                value.Substring(offset, 2),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture);
        }

        private static Vector4 ParseInlineVector4(string value)
        {
            Match match = InlineVector4.Match(value);
            if (!match.Success)
            {
                throw new GameplayMigrationDataException(
                    "Could not parse inline Vector4: " + value);
            }

            return new Vector4(
                float.Parse(match.Groups["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["z"].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["w"].Value, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        private static string EnsureRawSceneBackup(
            SceneSpec spec,
            string absoluteScenePath,
            string sourceHash)
        {
            string directory = Path.Combine(GetProjectDirectory(), BackupBase, spec.key);
            Directory.CreateDirectory(directory);
            string backupPath = Path.Combine(
                directory,
                Path.GetFileNameWithoutExtension(spec.scenePath) +
                ".before-legacy-static-batch." +
                sourceHash.Substring(0, 16) +
                ".unity");
            if (!File.Exists(backupPath))
            {
                File.Copy(absoluteScenePath, backupPath, false);
            }

            if (!string.Equals(
                    ComputeSha256(File.ReadAllBytes(backupPath)),
                    sourceHash,
                    StringComparison.Ordinal))
            {
                throw new IOException("Raw backup hash mismatch for " + spec.scenePath);
            }

            Debug.Log("[GameplayLegacyStaticBatch] exact raw scene backup: " + backupPath);
            return backupPath;
        }

        private static string GetGeneratedMeshPath(
            SceneSpec spec,
            RendererSubsetRecord record)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}_{2}.asset",
                spec.generatedMeshFolder,
                record.rendererFileId,
                SanitizeFileName(record.gameObjectName));
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            string[] segments = assetFolder.Split('/');
            if (segments.Length == 0 || segments[0] != "Assets")
            {
                throw new ArgumentException("Generated folder must be below Assets.", "assetFolder");
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

        private static string ComputeSha256(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(value => value.ToString("x2")));
            }
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            return Path.Combine(
                GetProjectDirectory(),
                assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string GetProjectDirectory()
        {
            string directory = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(directory))
            {
                throw new InvalidOperationException("Could not resolve Unity project directory.");
            }

            return directory;
        }

        private sealed class SceneSpec
        {
            public readonly string key;
            public readonly string scenePath;
            public readonly int expectedRendererCount;
            public readonly int expectedSelectedSubmeshCount;
            public readonly int expectedCombinedMeshCount;

            public string generatedRoot
            {
                get { return GeneratedBase + "/" + key; }
            }

            public string generatedMeshFolder
            {
                get { return generatedRoot + "/Meshes"; }
            }

            public string manifestPath
            {
                get { return generatedRoot + "/LegacySubsetManifest.json"; }
            }

            public SceneSpec(
                string key,
                string scenePath,
                int expectedRendererCount,
                int expectedSelectedSubmeshCount,
                int expectedCombinedMeshCount)
            {
                this.key = key;
                this.scenePath = scenePath;
                this.expectedRendererCount = expectedRendererCount;
                this.expectedSelectedSubmeshCount = expectedSelectedSubmeshCount;
                this.expectedCombinedMeshCount = expectedCombinedMeshCount;
            }
        }

        [Serializable]
        private sealed class GameplaySubsetManifest
        {
            public string sceneKey;
            public string scenePath;
            public string sceneGuid;
            public string originalSourceSha256;
            public string originalRawBackupPath;
            public int expectedRendererCount;
            public int expectedSelectedSubmeshCount;
            public int expectedCombinedMeshCount;
            public List<RendererSubsetRecord> renderers = new List<RendererSubsetRecord>();
        }

        [Serializable]
        private sealed class RendererSubsetRecord
        {
            public long rendererFileId;
            public long gameObjectFileId;
            public long meshFilterFileId;
            public string gameObjectName;
            public string subsetHexLittleEndian;
            public int[] submeshes;
            public string sourceMeshGuid;
            public long sourceMeshFileId;
            public int sourceLightmapIndex;
            public int lightmapIndex;
            public Vector4 lightmapScaleOffset;
            public string generatedMeshPath;
            public string generatedMeshGuid;
            public int generatedVertexCount;
            public int generatedIndexCount;
            public int materialCount;
            public int lodReferenceCount;
            public Vector3 sourceWorldBoundsCenter;
            public Vector3 sourceWorldBoundsExtents;
            public Vector3 generatedLocalBoundsCenter;
            public Vector3 generatedLocalBoundsExtents;
            public float maxPositionRoundTripError;
        }

        private sealed class PendingRendererRecord
        {
            public long rendererFileId;
            public long gameObjectFileId;
            public string subsetHex;
            public int sourceLightmapIndex;
            public Vector4 lightmapScaleOffset;
        }

        private sealed class MeshFilterRecord
        {
            public long meshFilterFileId;
            public long gameObjectFileId;
            public string sourceMeshGuid;
            public long sourceMeshFileId;
        }

        private sealed class StagedMigration
        {
            public readonly RendererSubsetRecord record;
            public readonly MeshRenderer renderer;
            public readonly MeshFilter filter;
            public readonly Mesh generatedMesh;
            public readonly string generatedPath;
            public readonly LegacyStaticBatchMigration.ReusableExtractionResult extraction;
            public readonly bool alreadyComplete;

            public StagedMigration(
                RendererSubsetRecord record,
                MeshRenderer renderer,
                MeshFilter filter,
                Mesh generatedMesh,
                string generatedPath,
                LegacyStaticBatchMigration.ReusableExtractionResult extraction)
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
                this.generatedMesh = filter.sharedMesh;
                this.generatedPath = generatedPath;
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

            public string Describe(
                string scenePath,
                StaticEditorFlags flagsBefore,
                StaticEditorFlags flagsAfter)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "[GameplayLegacyStaticBatch] migrated scene={0}, renderer={1}, " +
                    "gameObject={2}, name={3}, sourceMesh={4}/{5}, submeshes=[{6}], " +
                    "vertices={7}, indices={8}, materials={9}, lightmapIndex={10}, " +
                    "lightmapST={11}, lodRefs={12}, staticFlags={13}->{14}, " +
                    "sourceWorldBounds={15}, localBounds={16}, maxRoundTripError={17:R}, asset={18}",
                    scenePath,
                    record.rendererFileId,
                    record.gameObjectFileId,
                    record.gameObjectName,
                    record.sourceMeshGuid,
                    record.sourceMeshFileId,
                    string.Join(",", record.submeshes),
                    extraction.vertexCount,
                    extraction.indexCount,
                    record.materialCount,
                    record.lightmapIndex,
                    record.lightmapScaleOffset,
                    record.lodReferenceCount,
                    flagsBefore,
                    flagsAfter,
                    extraction.sourceWorldBounds,
                    extraction.outputLocalBounds,
                    extraction.maxRoundTripError,
                    generatedPath);
            }
        }

        private sealed class RendererState
        {
            public Material[] materials;
            public int lodReferenceCount;
            public bool enabled;
            public ShadowCastingMode shadowCastingMode;
            public bool receiveShadows;
            public LightProbeUsage lightProbeUsage;
            public ReflectionProbeUsage reflectionProbeUsage;
            public int sortingLayerId;
            public int sortingOrder;
            public StaticEditorFlags staticFlags;

            public static RendererState Capture(Scene scene, MeshRenderer renderer)
            {
                return new RendererState
                {
                    materials = renderer.sharedMaterials,
                    lodReferenceCount = CountLodReferences(scene, renderer),
                    enabled = renderer.enabled,
                    shadowCastingMode = renderer.shadowCastingMode,
                    receiveShadows = renderer.receiveShadows,
                    lightProbeUsage = renderer.lightProbeUsage,
                    reflectionProbeUsage = renderer.reflectionProbeUsage,
                    sortingLayerId = renderer.sortingLayerID,
                    sortingOrder = renderer.sortingOrder,
                    staticFlags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject)
                };
            }
        }

        private sealed class SceneMigrationResult
        {
            public readonly int changedCount;
            public readonly int validatedCount;

            public SceneMigrationResult(int changedCount, int validatedCount)
            {
                this.changedCount = changedCount;
                this.validatedCount = validatedCount;
            }
        }

        private sealed class GameplayMigrationDataException : Exception
        {
            public GameplayMigrationDataException(string message)
                : base(message)
            {
            }
        }
    }
}
#endif
