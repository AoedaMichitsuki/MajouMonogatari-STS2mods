using MajouMonogatari_STS2mods.Shared.ArtController;
using MajouMonogatari_STS2mods.Shared.Core;
using MajouMonogatari_STS2mods.Shared.Hooks;

namespace MajouMonogatari_STS2mods.ModEntry;

/// <summary>
/// Mod 启动编排器：统一维护初始化顺序与幂等控制。
/// </summary>
public static class ModBootstrap
{
    private static bool _initialized;
    private static int _attemptCount;

    public static bool InitializeOnce(string source)
    {
        if (_initialized)
        {
            return true;
        }

        _attemptCount++;
        ModLog.Prefix = $"[{ModConstants.ModId}]";
        ModLog.Info($"Initialization attempt {_attemptCount} ({source}).");

        ModelBootstrap.RegisterAll();
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
