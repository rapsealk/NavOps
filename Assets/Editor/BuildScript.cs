using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    static void BuildWindows()
    {
        Console.Out.WriteLine("[LOG] BuildWindows Start!");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        List<string> enabledScenePath = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) { continue; }

            enabledScenePath.Add(scene.path);
            Console.Out.WriteLine($"[LOG] EnableBuildScene: {scene}");
        }

        string binaryFilePath = UnityEngine.Application.dataPath + "/../" + "Binary/Windows/Rimpac.exe";
        if (!File.Exists(binaryFilePath))
        {
            FileInfo fileInfo = new FileInfo(binaryFilePath);
            fileInfo.Directory.CreateSubdirectory(fileInfo.DirectoryName);
        }

        BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        BuildOptions buildOptions = BuildOptions.None;
        Console.Out.WriteLine($"[LOG] Binary Path: {binaryFilePath}");
        Console.Out.WriteLine($"[LOG] Build Target: {buildTarget}");
        Console.Out.WriteLine($"[LOG] Build Options: {buildOptions}");

        BuildReport buildReport = BuildPipeline.BuildPlayer(enabledScenePath.ToArray(), binaryFilePath, buildTarget, buildOptions);
        Console.Out.WriteLine($"[LOG] Build Result: {buildReport.summary}");
    }
}
