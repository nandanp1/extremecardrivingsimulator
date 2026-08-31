#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace RacingConversion.Editor
{
    /// <summary>
    /// Reproducible ARM64 build settings for the reconstructed project.
    /// This creates genuine AArch64 Unity/IL2CPP code; it never relabels ARMv7 binaries.
    /// </summary>
    internal static class Arm64Build
    {
        private const string TestApplicationIdentifier = "com.aim.racing.arm64test";
        private const string DevelopmentBuildEnvironmentVariable =
            "RACING_DEVELOPMENT_BUILD";
        private const string RelativeOutputPath =
            "Builds/Android/ExtremeCarDrivingSimulator-arm64-test.apk";
        private const string PlayerSettingsAssetPath =
            "ProjectSettings/ProjectSettings.asset";
        private const int AndroidFixedDpiResolutionScalingMode = 1;
        private const int AndroidTargetPixelDensity = 320;
        private const float AndroidResolutionScalingFixedDpiFactor = 1f;

        private static string ProjectDirectory
        {
            get
            {
                string projectDirectory = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectDirectory))
                {
                    throw new InvalidOperationException(
                        "Could not resolve the Unity project directory from Application.dataPath.");
                }

                return projectDirectory;
            }
        }

        private static string OutputPath
        {
            get { return Path.Combine(ProjectDirectory, RelativeOutputPath); }
        }

        [MenuItem("Racing Conversion/Configure Android ARM64")]
        public static void ConfigureAndroidArm64()
        {
            ConfigureAndroidArm64(false);
        }

        private static void ConfigureAndroidArm64(bool developmentBuild)
        {
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android,
                ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.Android.bundleVersionCode = 20070;
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            PlayerSettings.Android.splitApplicationBinary = false;
            PlayerSettings.Android.minifyDebug = false;
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.useCustomKeystore = false;

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                TestApplicationIdentifier);
            PlayerSettings.bundleVersion = "4.17.6-reconstructed";

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(
                BuildTarget.Android,
                new[] { GraphicsDeviceType.OpenGLES3 });

            // The Pixel 9 Pro XL exposes a 2992x1344 framebuffer at 480 dpi.
            // This Unity 4-era game does not benefit from shading four million
            // pixels per frame. Fixed DPI applies one uniform scale, retaining
            // the native aspect ratio and the NGUI anchor/touch coordinate model,
            // while 320 dpi renders about 1995x896 on that device.
            ConfigureAndroidFixedDpiResolution();

            // The recovered Unity 4 worlds were authored as static batches. The
            // migration first expands each stale legacy batch into ordinary,
            // independently cullable renderers so Unity 6 can render the correct
            // submeshes. Rebatch those verified renderers during the player build
            // to recover the original draw-call profile without changing their
            // transforms, materials, lightmaps, or collision meshes.
            PlayerSettings.SetStaticBatchingForPlatform(BuildTarget.Android, true);

            // Keep stripping conservative until reflection-heavy legacy SDK code is replaced.
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetManagedStrippingLevel(
                NamedBuildTarget.Android,
                ManagedStrippingLevel.Minimal);

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidBuildType = developmentBuild
                ? AndroidBuildType.Development
                : AndroidBuildType.Release;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            AssetDatabase.SaveAssets();

            Debug.Log("Configured Android IL2CPP, ARM64-only, OpenGL ES 3, " +
                      (developmentBuild ? "development" : "release") +
                      " test package " + TestApplicationIdentifier + ".");
        }

        [MenuItem("Racing Conversion/Build ARM64 Development APK")]
        public static void BuildDevelopmentApk()
        {
            BuildApk(true);
        }

        [MenuItem("Racing Conversion/Build ARM64 Release APK")]
        public static void BuildReleaseApk()
        {
            BuildApk(false);
        }

        private static void BuildApk(bool developmentBuild)
        {
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildTargetGroup.Android,
                    BuildTarget.Android))
            {
                throw new BuildFailedException(
                    "Android Build Support is not installed for this Unity editor.");
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                throw new InvalidOperationException(
                    "Android must be the active build target. In batch mode pass '-buildTarget Android'.");
            }

            ConfigureAndroidArm64(developmentBuild);
            ValidateBuildConfiguration(developmentBuild);
            ValidateNativePlugins();

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled build scenes were found.");
            }

            string[] missingScenes = scenes
                .Where(scene => !File.Exists(Path.Combine(ProjectDirectory, scene)))
                .ToArray();
            if (missingScenes.Length > 0)
            {
                throw new BuildFailedException(
                    "Enabled build scenes are missing from disk:\n" +
                    string.Join("\n", missingScenes));
            }

            string outputPath = OutputPath;
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Never allow a stale APK to be mistaken for the result of this invocation.
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = developmentBuild
                    ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None
            });

            if (report == null)
            {
                throw new BuildFailedException(
                    "Unity returned no BuildReport for the Android ARM64 build.");
            }

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    "Android ARM64 build failed: " + report.SummarizeErrors());
            }

            if (!File.Exists(outputPath))
            {
                throw new BuildFailedException(
                    "Unity reported success but the expected APK was not created at " + outputPath);
            }

            Debug.Log("ARM64 " + (developmentBuild ? "development" : "release") +
                      " APK built at " + outputPath);
        }

        public static void BuildFromCommandLine()
        {
            string developmentValue = Environment.GetEnvironmentVariable(
                DevelopmentBuildEnvironmentVariable);
            bool developmentBuild = string.Equals(
                developmentValue,
                "1",
                StringComparison.OrdinalIgnoreCase) || string.Equals(
                developmentValue,
                "true",
                StringComparison.OrdinalIgnoreCase);
            BuildApk(developmentBuild);
        }

        private static void ValidateBuildConfiguration(bool developmentBuild)
        {
            var errors = new List<string>();

            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) !=
                ScriptingImplementation.IL2CPP)
            {
                errors.Add("Android scripting backend is not IL2CPP.");
            }

            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                errors.Add("Android target architecture is not ARM64-only.");
            }

            if (PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel25)
            {
                errors.Add("Android minimum SDK is not API 25.");
            }

            if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel36)
            {
                errors.Add("Android target SDK is not API 36.");
            }

            if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) !=
                TestApplicationIdentifier)
            {
                errors.Add("Android application identifier is not the migration test identifier.");
            }

            GraphicsDeviceType[] graphicsApis =
                PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            if (graphicsApis.Length != 1 || graphicsApis[0] != GraphicsDeviceType.OpenGLES3)
            {
                errors.Add("Android graphics API list is not OpenGL ES 3 only.");
            }

            if (!PlayerSettings.GetStaticBatchingForPlatform(BuildTarget.Android))
            {
                errors.Add("Android static batching is disabled.");
            }

            if (PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.Android) !=
                ManagedStrippingLevel.Minimal)
            {
                errors.Add("Managed stripping is not set to Minimal.");
            }

            ValidateAndroidFixedDpiResolution(errors);

            if (PlayerSettings.Android.useCustomKeystore)
            {
                errors.Add("A custom keystore is enabled; this test build must use debug signing.");
            }

            if (PlayerSettings.Android.splitApplicationBinary)
            {
                errors.Add("Split application binary/OBB output is enabled.");
            }

            if (PlayerSettings.Android.buildApkPerCpuArchitecture)
            {
                errors.Add("Per-CPU APK splitting is enabled.");
            }

            if (PlayerSettings.Android.minifyDebug)
            {
                errors.Add("Debug minification is enabled during migration validation.");
            }

            if (PlayerSettings.stripEngineCode)
            {
                errors.Add("Engine-code stripping is enabled during migration validation.");
            }

            if (EditorUserBuildSettings.androidBuildSystem != AndroidBuildSystem.Gradle)
            {
                errors.Add("Android build system is not Gradle.");
            }

            AndroidBuildType expectedBuildType = developmentBuild
                ? AndroidBuildType.Development
                : AndroidBuildType.Release;
            if (EditorUserBuildSettings.androidBuildType != expectedBuildType)
            {
                errors.Add("Android build type is not " + expectedBuildType + ".");
            }

            if (EditorUserBuildSettings.buildAppBundle)
            {
                errors.Add("Android App Bundle output is enabled instead of APK output.");
            }

            if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
            {
                errors.Add("Gradle project export is enabled instead of direct APK output.");
            }

            if (errors.Count > 0)
            {
                throw new BuildFailedException(
                    "Android ARM64 configuration validation failed:\n" +
                    string.Join("\n", errors));
            }
        }

        private static void ConfigureAndroidFixedDpiResolution()
        {
            SerializedObject playerSettings = GetSerializedPlayerSettings();
            SerializedProperty scalingMode =
                playerSettings.FindProperty("resolutionScalingMode");
            SerializedProperty targetPixelDensity =
                playerSettings.FindProperty("targetPixelDensity");

            if (scalingMode == null || targetPixelDensity == null)
            {
                throw new BuildFailedException(
                    "Unity's Android resolution-scaling PlayerSettings fields were not found.");
            }

            scalingMode.intValue = AndroidFixedDpiResolutionScalingMode;
            targetPixelDensity.intValue = AndroidTargetPixelDensity;
            playerSettings.ApplyModifiedPropertiesWithoutUndo();

            // Unity multiplies target DPI by this per-quality factor before
            // calculating min(targetDpi * factor / screenDpi, 1).
            QualitySettings.resolutionScalingFixedDPIFactor =
                AndroidResolutionScalingFixedDpiFactor;
        }

        private static void ValidateAndroidFixedDpiResolution(List<string> errors)
        {
            SerializedObject playerSettings = GetSerializedPlayerSettings();
            SerializedProperty scalingMode =
                playerSettings.FindProperty("resolutionScalingMode");
            SerializedProperty targetPixelDensity =
                playerSettings.FindProperty("targetPixelDensity");

            if (scalingMode == null ||
                scalingMode.intValue != AndroidFixedDpiResolutionScalingMode)
            {
                errors.Add("Android resolution scaling is not Fixed DPI.");
            }

            if (targetPixelDensity == null ||
                targetPixelDensity.intValue != AndroidTargetPixelDensity)
            {
                errors.Add("Android target pixel density is not " +
                           AndroidTargetPixelDensity + " DPI.");
            }

            if (!Mathf.Approximately(
                    QualitySettings.resolutionScalingFixedDPIFactor,
                    AndroidResolutionScalingFixedDpiFactor))
            {
                errors.Add("Resolution Scaling Fixed DPI Factor is not 1.0.");
            }
        }

        private static SerializedObject GetSerializedPlayerSettings()
        {
            UnityEngine.Object[] settingsAssets =
                AssetDatabase.LoadAllAssetsAtPath(PlayerSettingsAssetPath);
            if (settingsAssets == null || settingsAssets.Length == 0 ||
                settingsAssets[0] == null)
            {
                throw new BuildFailedException(
                    "Could not load " + PlayerSettingsAssetPath + ".");
            }

            var playerSettings = new SerializedObject(settingsAssets[0]);
            playerSettings.Update();
            return playerSettings;
        }

        private static void ValidateNativePlugins()
        {
            string[] nativePlugins = Directory
                .GetFiles(Application.dataPath, "*", SearchOption.AllDirectories)
                .Where(path => string.Equals(
                    Path.GetExtension(path),
                    ".so",
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var violations = new List<string>();
            foreach (string path in nativePlugins)
            {
                string normalizedPath = path.Replace('\\', '/');
                if (!normalizedPath.Contains("/arm64-v8a/"))
                {
                    violations.Add(path + ": not stored under an arm64-v8a directory");
                    continue;
                }

                string elfProblem;
                if (!IsAarch64Elf(path, out elfProblem))
                {
                    violations.Add(path + ": " + elfProblem);
                }
            }

            if (violations.Count > 0)
            {
                throw new BuildFailedException(
                    "Native Android plugin validation failed:\n" +
                    string.Join("\n", violations));
            }
        }

        private static bool IsAarch64Elf(string path, out string problem)
        {
            // ELF identification fields through e_machine occupy the first 20 bytes.
            var header = new byte[20];
            using (FileStream stream = File.OpenRead(path))
            {
                int offset = 0;
                while (offset < header.Length)
                {
                    int read = stream.Read(header, offset, header.Length - offset);
                    if (read == 0)
                    {
                        problem = "file is too short to contain an ELF header";
                        return false;
                    }

                    offset += read;
                }
            }

            if (header[0] != 0x7f || header[1] != (byte)'E' ||
                header[2] != (byte)'L' || header[3] != (byte)'F')
            {
                problem = "file does not have ELF magic";
                return false;
            }

            if (header[4] != 2)
            {
                problem = "ELF class is not ELF64";
                return false;
            }

            if (header[5] != 1)
            {
                problem = "ELF byte order is not little-endian";
                return false;
            }

            int machine = header[18] | (header[19] << 8);
            if (machine != 183)
            {
                problem = "ELF e_machine is " + machine + ", not AArch64 (183)";
                return false;
            }

            problem = null;
            return true;
        }
    }
}
#endif
