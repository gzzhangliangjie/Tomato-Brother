using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CommandLineBuild
{
    public static void BuildWin64()
    {
        var scenes = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.enabled)
                scenes.Add(s.path);
        }
        if (scenes.Count == 0)
            scenes.Add("Assets/Scenes/Start.unity");

        var opts = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = "Builds/Windows/TomatoBrother.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("BUILD FAILED: " + report.summary.result +
                           ", errors: " + report.summary.totalErrors);
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log("BUILD OK, size: " + report.summary.totalSize + " bytes");
        }
    }
}
