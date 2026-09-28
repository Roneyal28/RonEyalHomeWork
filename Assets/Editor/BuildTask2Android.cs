using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildTask2Android
{
    [MenuItem("Tools/Task 2/Build Android Development APK")]
    public static void Build()
    {
        string output = Path.GetFullPath("Builds/Task2/Task2.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.development = true;
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Character Selection.unity", "Assets/Scenes/ArsenalArmory.unity" },
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.Development
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        File.WriteAllText("/tmp/unity-task2-android-build.txt",
            report.summary.result + "\n" + report.summary.totalErrors + " errors\n" + report.summary.totalWarnings + " warnings\n" + output);
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Task 2 Android build failed: " + report.summary.result);
        Debug.Log("TASK2_ANDROID_BUILD_SUCCEEDED " + output);
    }
}
