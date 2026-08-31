#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RacingConversion.Editor
{
    /// <summary>
    /// Reapplies lightmap bindings that Unity 6 strips when it upgrades the
    /// reconstructed legacy static-batched scenes. IProcessSceneWithReport
    /// receives the temporary scene instance that will be serialized into the
    /// player, so the original scene asset is never changed or saved here.
    /// </summary>
    public sealed class LegacyStaticBatchLightmapBuildProcessor : IProcessSceneWithReport
    {
        private static readonly BuildSceneSpec[] SupportedScenes =
        {
            new BuildSceneSpec(
                "Assets/Scenes/Common/1Menu.unity",
                "Assets/Reconstruction/Generated/1MenuLegacyStaticBatch/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/1MenuLegacyStaticBatch/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Common/2Game.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Common_2Game/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Common_2Game/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_City.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_City/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_City/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone0.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport_Zone0/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport_Zone0/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone1.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport_Zone1/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport_Zone1/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Airport_Zone2.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport_Zone2/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Airport_Zone2/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone0.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone0/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone0/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone1.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone1/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone1/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone2.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone2/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone2/Meshes/"),
            new BuildSceneSpec(
                "Assets/Scenes/Games/Extreme/AsyncScenes/2Game_Zone3.unity",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone3/LegacySubsetManifest.json",
                "Assets/Reconstruction/Generated/GameplayLegacyStaticBatches/Extreme_2Game_Zone3/Meshes/")
        };

        // UnityBuildPostprocessor performs the editor's static mesh combine at
        // callback order 0. Restore the lightmap bindings and BatchingStatic flag
        // first so the combiner sees the migrated renderers and groups them using
        // their real lightmap index/ST. Running after order 0 merely leaves a flag
        // on the serialized renderer; it does not create a player static batch.
        public int callbackOrder
        {
            get { return -10000; }
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // Unity can invoke scene callbacks outside a player build with a null
            // report. This recovery must only affect an actual player build.
            if (report == null)
            {
                return;
            }

            BuildSceneSpec spec = FindSceneSpec(scene.path);
            if (spec == null)
            {
                return;
            }

            int appliedCount = ValidateAndOptionallyApply(scene, spec, true);
            Debug.Log(string.Format(
                CultureInfo.InvariantCulture,
                "[LegacyStaticBatchLightmap] Applied {0} recovered lightmap bindings " +
                "to the temporary player-build scene {1}; source scene was not saved.",
                appliedCount,
                spec.scenePath));
        }

        /// <summary>
        /// Batch-mode compile/import verification used without building an APK.
        /// It opens each migrated scene, validates all generated-mesh/local-ID/
        /// manifest mappings, performs no assignments, and never saves a scene.
        /// </summary>
        public static void ValidateConfigurationFromCommandLine()
        {
            int totalValidatedCount = 0;
            foreach (BuildSceneSpec spec in SupportedScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(spec.scenePath, OpenSceneMode.Single);
                int validatedCount = ValidateAndOptionallyApply(scene, spec, false);
                totalValidatedCount += validatedCount;
                Debug.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[LegacyStaticBatchLightmap] Dry-run validated {0} generated renderer " +
                    "bindings for {1}; no scene values were changed or saved.",
                    validatedCount,
                    spec.scenePath));
            }

            Debug.Log(string.Format(
                CultureInfo.InvariantCulture,
                "[LegacyStaticBatchLightmap] Dry-run validated {0} generated renderer " +
                "bindings across {1} supported scenes; no scene values were changed or saved.",
                totalValidatedCount,
                SupportedScenes.Length));
        }

        private static int ValidateAndOptionallyApply(
            Scene scene,
            BuildSceneSpec spec,
            bool apply)
        {
            if (!scene.IsValid() ||
                !string.Equals(scene.path, spec.scenePath, StringComparison.Ordinal))
            {
                throw new BuildFailedException(
                    "Legacy lightmap recovery received an unexpected scene: " + scene.path);
            }

            LegacySubsetManifest manifest = LoadAndValidateManifest(spec);
            var recordsByRendererId = new Dictionary<long, RendererSubsetRecord>();
            foreach (RendererSubsetRecord record in manifest.renderers)
            {
                if (record == null)
                {
                    throw new BuildFailedException(
                        "The legacy subset manifest contains a null renderer record.");
                }

                if (recordsByRendererId.ContainsKey(record.rendererFileId))
                {
                    throw new BuildFailedException(string.Format(
                        CultureInfo.InvariantCulture,
                        "The legacy subset manifest ambiguously contains renderer local ID {0} twice.",
                        record.rendererFileId));
                }

                ValidateLightmapRecord(record);
                recordsByRendererId.Add(record.rendererFileId, record);
            }

            var generatedRenderersById = new Dictionary<long, MeshRenderer>();
            int appliedCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null)
                    {
                        continue;
                    }

                    string meshPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                    if (string.IsNullOrEmpty(meshPath) ||
                        !meshPath.StartsWith(spec.generatedMeshFolder, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    long rendererLocalId = GetRequiredRendererLocalId(renderer, meshPath);
                    MeshRenderer duplicateRenderer;
                    if (generatedRenderersById.TryGetValue(rendererLocalId, out duplicateRenderer))
                    {
                        throw new BuildFailedException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Generated migrated meshes map ambiguously to renderer local ID {0}: " +
                            "{1} and {2}.",
                            rendererLocalId,
                            duplicateRenderer.name,
                            renderer.name));
                    }

                    generatedRenderersById.Add(rendererLocalId, renderer);

                    RendererSubsetRecord record;
                    if (!recordsByRendererId.TryGetValue(rendererLocalId, out record))
                    {
                        throw new BuildFailedException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Generated migrated mesh {0} is used by renderer {1} (local ID {2}), " +
                            "but the preserved manifest has no matching record.",
                            meshPath,
                            renderer.name,
                            rendererLocalId));
                    }

                    string expectedFilePrefix =
                        rendererLocalId.ToString(CultureInfo.InvariantCulture) + "_";
                    if (!Path.GetFileName(meshPath).StartsWith(
                            expectedFilePrefix,
                            StringComparison.Ordinal))
                    {
                        throw new BuildFailedException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Generated mesh filename/local-ID mismatch: renderer {0}, local ID {1}, " +
                            "asset {2}.",
                            renderer.name,
                            rendererLocalId,
                            meshPath));
                    }

                    if (record.gameObjectFileId != GetRequiredGameObjectLocalId(
                            renderer.gameObject,
                            meshPath))
                    {
                        throw new BuildFailedException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Manifest GameObject local ID mismatch for renderer {0} ({1}).",
                            rendererLocalId,
                            meshPath));
                    }

                    if (apply)
                    {
                        renderer.lightmapIndex = record.lightmapIndex;
                        renderer.lightmapScaleOffset = record.lightmapScaleOffset;

                        // The source renderer belonged to a Unity 4 static batch.
                        // Its stale batch bookkeeping was deliberately removed by
                        // the mesh migration. This temporary build-scene copy now
                        // owns a normal, local-space generated mesh, so it is safe
                        // to let Unity 6 create a fresh batch for the player. Keep
                        // the source scene asset unmodified and preserve every
                        // other static flag.
                        StaticEditorFlags flags =
                            GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                        GameObjectUtility.SetStaticEditorFlags(
                            renderer.gameObject,
                            flags | StaticEditorFlags.BatchingStatic);

                        if (renderer.lightmapIndex != record.lightmapIndex ||
                            renderer.lightmapScaleOffset != record.lightmapScaleOffset)
                        {
                            throw new BuildFailedException(string.Format(
                                CultureInfo.InvariantCulture,
                                "Unity rejected the recovered lightmap binding for renderer {0} ({1}).",
                                rendererLocalId,
                                meshPath));
                        }

                        if (!GameObjectUtility.AreStaticEditorFlagsSet(
                                renderer.gameObject,
                                StaticEditorFlags.BatchingStatic))
                        {
                            throw new BuildFailedException(string.Format(
                                CultureInfo.InvariantCulture,
                                "Unity rejected BatchingStatic for renderer {0} ({1}).",
                                rendererLocalId,
                                meshPath));
                        }
                    }

                    appliedCount++;
                }
            }

            // Every preserved renderer must still exist in the scene and use its
            // generated local mesh. The checks above reject unexpected generated
            // renderers, so equal cardinality proves the mapping is one-to-one.
            // Without this guard, deleting one migrated renderer (or reverting its
            // MeshFilter) would make the dry run report a smaller success count and
            // silently omit the corresponding lightmap/static-batching recovery.
            if (generatedRenderersById.Count != recordsByRendererId.Count)
            {
                throw new BuildFailedException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Legacy static-batch manifest/scene renderer count mismatch for {0}: " +
                    "manifest={1}, generated scene renderers={2}.",
                    spec.scenePath,
                    recordsByRendererId.Count,
                    generatedRenderersById.Count));
            }

            return appliedCount;
        }

        private static LegacySubsetManifest LoadAndValidateManifest(BuildSceneSpec spec)
        {
            string absoluteManifestPath = Path.Combine(
                Path.GetDirectoryName(Application.dataPath) ?? string.Empty,
                spec.manifestPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absoluteManifestPath))
            {
                throw new BuildFailedException(
                    "Legacy static-batch manifest is missing: " + absoluteManifestPath);
            }

            LegacySubsetManifest manifest;
            try
            {
                manifest = JsonUtility.FromJson<LegacySubsetManifest>(
                    File.ReadAllText(absoluteManifestPath));
            }
            catch (Exception exception)
            {
                throw new BuildFailedException(
                    "Could not parse legacy static-batch manifest: " + exception.Message);
            }

            if (manifest == null || manifest.renderers == null || manifest.renderers.Count == 0)
            {
                throw new BuildFailedException(
                    "Legacy static-batch manifest has no renderer records: " + spec.manifestPath);
            }

            if (!string.Equals(manifest.scenePath, spec.scenePath, StringComparison.Ordinal))
            {
                throw new BuildFailedException(
                    "Legacy static-batch manifest scene path does not match " + spec.scenePath);
            }

            string currentSceneGuid = AssetDatabase.AssetPathToGUID(spec.scenePath);
            if (string.IsNullOrEmpty(currentSceneGuid) ||
                !string.Equals(manifest.sceneGuid, currentSceneGuid, StringComparison.Ordinal))
            {
                throw new BuildFailedException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Legacy static-batch manifest scene GUID mismatch: manifest={0}, current={1}.",
                    manifest.sceneGuid,
                    currentSceneGuid));
            }

            return manifest;
        }

        private static BuildSceneSpec FindSceneSpec(string scenePath)
        {
            foreach (BuildSceneSpec spec in SupportedScenes)
            {
                if (string.Equals(spec.scenePath, scenePath, StringComparison.Ordinal))
                {
                    return spec;
                }
            }

            return null;
        }

        private static void ValidateLightmapRecord(RendererSubsetRecord record)
        {
            if (record.rendererFileId == 0 || record.gameObjectFileId == 0)
            {
                throw new BuildFailedException(
                    "Legacy static-batch manifest contains a zero object local ID.");
            }

            if (record.lightmapIndex < -1)
            {
                throw new BuildFailedException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Renderer {0} has invalid recovered lightmap index {1}.",
                    record.rendererFileId,
                    record.lightmapIndex));
            }

            Vector4 scaleOffset = record.lightmapScaleOffset;
            if (!IsFinite(scaleOffset.x) ||
                !IsFinite(scaleOffset.y) ||
                !IsFinite(scaleOffset.z) ||
                !IsFinite(scaleOffset.w))
            {
                throw new BuildFailedException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Renderer {0} has a non-finite recovered lightmap scale/offset.",
                    record.rendererFileId));
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static long GetRequiredRendererLocalId(MeshRenderer renderer, string meshPath)
        {
            long localId = GetLocalId(renderer);
            if (localId == 0)
            {
                throw new BuildFailedException(
                    "Could not resolve a persistent renderer local ID for generated mesh " + meshPath);
            }

            return localId;
        }

        private static long GetRequiredGameObjectLocalId(GameObject gameObject, string meshPath)
        {
            long localId = GetLocalId(gameObject);
            if (localId == 0)
            {
                throw new BuildFailedException(
                    "Could not resolve a persistent GameObject local ID for generated mesh " + meshPath);
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

        [Serializable]
        private sealed class LegacySubsetManifest
        {
            public string scenePath;
            public string sceneGuid;
            public List<RendererSubsetRecord> renderers = new List<RendererSubsetRecord>();
        }

        [Serializable]
        private sealed class RendererSubsetRecord
        {
            public long rendererFileId;
            public long gameObjectFileId;
            public int lightmapIndex;
            public Vector4 lightmapScaleOffset;
        }

        private sealed class BuildSceneSpec
        {
            public readonly string scenePath;
            public readonly string manifestPath;
            public readonly string generatedMeshFolder;

            public BuildSceneSpec(
                string scenePath,
                string manifestPath,
                string generatedMeshFolder)
            {
                this.scenePath = scenePath;
                this.manifestPath = manifestPath;
                this.generatedMeshFolder = generatedMeshFolder;
            }
        }
    }
}
#endif
