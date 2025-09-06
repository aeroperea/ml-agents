using UnityEditor;
using UnityEditor.Build;                // NamedBuildTarget
using UnityEngine;
using UnityEngine.Rendering;            // GraphicsSettings

public static class HeadlessSettings
{
    [MenuItem("Build/Apply Headless Settings")]
    public static void Apply()
    {
        // Use NamedBuildTarget for player settings
        var nbt = NamedBuildTarget.Standalone;

        // Scripting backend: Mono
        PlayerSettings.SetScriptingBackend(nbt, ScriptingImplementation.Mono2x);

        // API compatibility level
#if UNITY_2021_2_OR_NEWER
        PlayerSettings.SetApiCompatibilityLevel(nbt, ApiCompatibilityLevel.NET_Standard_2_0);
#else
        PlayerSettings.SetApiCompatibilityLevel(nbt, ApiCompatibilityLevel.NET_4_6);
#endif

        PlayerSettings.SetManagedStrippingLevel(nbt, ManagedStrippingLevel.Low);
        PlayerSettings.stripEngineCode = false;

        // Graphics APIs must use BuildTarget
        var linux = BuildTarget.StandaloneLinux64;
        PlayerSettings.SetUseDefaultGraphicsAPIs(linux, false);
        PlayerSettings.SetGraphicsAPIs(linux, new[]
        {
            GraphicsDeviceType.Vulkan,
            GraphicsDeviceType.OpenGLCore
        });

        // Clear SRP (URP/HDRP) for headless
        GraphicsSettings.defaultRenderPipeline = null;

        // Also clear per-quality SRP bindings
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = null;
        }

        // Headless niceties
        QualitySettings.vSyncCount = 0;
        PlayerSettings.usePlayerLog = true;

        Debug.Log("[HeadlessSettings] Applied: Mono, NET Std 2.0, Vulkan/OpenGLCore, SRP cleared.");
    }
}
