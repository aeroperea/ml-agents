using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildCommandLine
{
    // [entrypoints]
    public static void BuildWin64() => BuildStandalone(BuildTarget.StandaloneWindows64);
    public static void BuildLinux64() => BuildStandalone(BuildTarget.StandaloneLinux64);

    // [core]
    private static void BuildStandalone(BuildTarget target)
    {
        // keep editor state minimal in batch builds
        try { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); } catch { }

        var args = Environment.GetCommandLineArgs();

        var buildPathArg = GetArg(args, "-buildPath");
        var headless = HasArg(args, "-headless");
        var dev = HasArg(args, "-dev");
        var forceMono = HasArg(args, "-forceMono");

        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var defaultOut = target == BuildTarget.StandaloneWindows64
            ? Path.Combine(projectRoot, "Builds", "Win64", PlayerSettings.productName + ".exe")
            : Path.Combine(projectRoot, "Builds", "Linux64", PlayerSettings.productName);

        var outPath = string.IsNullOrWhiteSpace(buildPathArg) ? defaultOut : ToFullPath(projectRoot, buildPathArg);
        EnsureParentDirExists(outPath);

        // scenes from build settings (your screenshot shows one: pelletgrabber)
        var scenes = EditorBuildSettings.scenes
            .Where(s => s != null && s.enabled)
            .Select(s => s.path)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("no enabled scenes found in build settings.");

        if (forceMono)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        var options = BuildOptions.None;
        if (headless) options |= BuildOptions.EnableHeadlessMode;
        if (dev) options |= BuildOptions.Development;

        Debug.Log($"[cli-build] unity={Application.unityVersion}");
        Debug.Log($"[cli-build] target={target} out={outPath}");
        Debug.Log($"[cli-build] scenes={string.Join(", ", scenes)}");
        Debug.Log($"[cli-build] headless={headless} dev={dev} forceMono={forceMono}");

        var bp = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outPath,
            target = target,
            options = options
        };

        var report = BuildPipeline.BuildPlayer(bp);

        Debug.Log($"[cli-build] result={report.summary.result} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings}");
        Debug.Log($"[cli-build] totalTime={report.summary.totalTime} totalSize={report.summary.totalSize} bytes");

        // nonzero exit for ci/scripts
        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);

        EditorApplication.Exit(0);
    }

    // [args helpers]
    private static bool HasArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static string GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    private static string ToFullPath(string projectRoot, string path)
    {
        if (Path.IsPathRooted(path)) return path;
        return Path.GetFullPath(Path.Combine(projectRoot, path));
    }

    private static void EnsureParentDirExists(string outPath)
    {
        var dir = Path.GetDirectoryName(outPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
