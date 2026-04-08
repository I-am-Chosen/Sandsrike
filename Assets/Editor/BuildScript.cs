using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Dedicated Server build script.
/// Run from command line:
///   Unity.exe -batchmode -nographics -projectPath . -executeMethod BuildScript.BuildServer -quit
/// Output: ServerBuild/Sandsrike.x86_64
/// </summary>
public class BuildScript
{
    private const string OutputDir    = "ServerBuild";
    private const string BinaryName   = "Sandsrike.x86_64";

    public static void BuildServer()
    {
        Debug.Log("[BuildScript] Starting Dedicated Server build...");

        // Clean output directory (keep .gitkeep and .log files)
        if (Directory.Exists(OutputDir))
        {
            foreach (var file in Directory.GetFiles(OutputDir))
            {
                if (file.EndsWith(".gitkeep") || file.EndsWith(".log")) continue;
                try { File.Delete(file); } catch { /* skip locked files */ }
            }
            foreach (var dir in Directory.GetDirectories(OutputDir))
                try { Directory.Delete(dir, true); } catch { /* skip */ }
        }
        Directory.CreateDirectory(OutputDir);

        // Collect all scenes from Build Settings
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled)
                scenes.Add(scene.path);

        if (scenes.Count == 0)
        {
            // Fallback: explicit scene order
            scenes.AddRange(new[]
            {
                "Assets/Scenes/BootScene.unity",
                "Assets/Scenes/LoadingScene.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/GameScene.unity",
                // Demo excluded from server build
            });
        }

        Debug.Log($"[BuildScript] Scenes: {string.Join(", ", scenes)}");

        var options = new BuildPlayerOptions
        {
            scenes              = scenes.ToArray(),
            locationPathName    = Path.Combine(OutputDir, BinaryName),
            target              = BuildTarget.StandaloneLinux64,
            subtarget           = (int)StandaloneBuildSubtarget.Server,
            options             = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] ✓ Build succeeded | Size: {summary.totalSize / 1024 / 1024} MB | Time: {summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"[BuildScript] ✗ Build FAILED: {summary.result}");
            foreach (var step in report.steps)
                foreach (var msg in step.messages)
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"  {msg.content}");

            // Exit with error code so CI knows it failed
            EditorApplication.Exit(1);
        }
    }
}
