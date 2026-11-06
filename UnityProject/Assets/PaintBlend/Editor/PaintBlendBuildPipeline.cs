#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PaintBlend.Editor
{
    /// <summary>
    /// Reproducible Android build entry point for local command-line builds and CI.
    /// The generated scene is a foundation blockout; this does not imply release-art
    /// or full-game acceptance.
    /// </summary>
    public static class PaintBlendBuildPipeline
    {
        private const string ScenePath = "Assets/PaintBlend/Scenes/GrandHallComplex_Blockout.unity";

        public static void BuildAndroid()
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("BuildAndroid is intended for Unity batch mode.");

            string projectRoot = Directory.GetCurrentDirectory();
            string outputPath = Environment.GetEnvironmentVariable("PAINTBLEND_BUILD_PATH");
            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = Path.Combine(projectRoot, "Build", "Android", "PaintAndBlend.apk");

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Unity could not switch the active build target to Android.");

            // Generate the single connected-world foundation scene deterministically.
            GrandHallSceneBuilder.BuildGrandHallBlockout();

            if (!File.Exists(ScenePath))
                throw new FileNotFoundException("Generated Grand Hall scene was not saved.", ScenePath);

            PlayerSettings.productName = "Paint & Blend";
            PlayerSettings.applicationIdentifier = "com.radhengle.paintblend";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Android build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors +
                    "; warnings=" + report.summary.totalWarnings);
            }

            Debug.Log("Paint & Blend Android APK created: " + outputPath +
                      " (" + report.summary.totalSize + " bytes)");
        }
    }
}
#endif
