using MajouMonogatari_STS2mods.Shared.ArtController;
using MajouMonogatari_STS2mods.Shared.Core;
using MajouMonogatari_STS2mods.Shared.Hooks;
using MegaCrit.Sts2.Core.Modding;

namespace MajouMonogatari_STS2mods.ModEntry;

/// <summary>
/// Mod 启动编排器：统一维护初始化顺序与幂等控制。
/// </summary>
[ModInitializer(nameof(Initialize))]
public static class ModBootstrap
{
    private static bool _initialized;
    private static int _attemptCount;

    /// <summary>
    /// STS2 ModManager 入口：通过 ModInitializerAttribute 反射调用。
    /// </summary>
    public static void Initialize()
    {
        InitializeOnce("sts2-mod-initializer");
    }

    public static bool InitializeOnce(string source)
    {
        if (_initialized)
        {
            return true;
        }

        _attemptCount++;
        ModLog.Prefix = $"[{ModConstants.ModId}]";
        ModLog.Info($"Initialization attempt {_attemptCount} ({source}).");

        ArtManifestReporter.ReportMissing(CecilyArtProvider.Instance);

        if (!HookRegistry.RegisterAll(ModConstants.ModId))
        {
            ModLog.Warn("HookRegistry registration did not complete. Will retry on next entry point.");
            return false;
        }

        _initialized = true;
        ModLog.Info("Initialization completed.");
        return true;
    }
}
