using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Soulbreak.EditorTools
{
    // Soulbreak/Build WebGL: builds the game for the browser into Builds/WebGL, ready to zip for itch.io.
    public static class WebBuild
    {
        public const string Output = "Builds/WebGL";

        [MenuItem("Soulbreak/Build WebGL")]
        public static void Build()
        {
            // itch.io serves files as-is, so gzip + the built-in decompression fallback is the safe combo.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultWebScreenWidth = 1920;
            PlayerSettings.defaultWebScreenHeight = 1080;

            var scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/SampleScene.unity" };
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"WebGL build done: {Path.GetFullPath(Output)} ({summary.totalSize / (1024f * 1024f):0.0} MB). Zip the folder's contents and upload to itch.io.");
            else
                Debug.LogError($"WebGL build {summary.result}: {summary.totalErrors} error(s). See the console above.");
        }
    }
}
