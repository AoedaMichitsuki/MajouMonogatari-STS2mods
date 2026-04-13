using MajouMonogatari_STS2mods.Shared.Core;
using MajouMonogatari_STS2mods.Shared.Hooks;

namespace MajouMonogatari_STS2mods.ModEntry;

public static class ModBootstrap
{
    private static readonly object Gate = new();
    private static bool _initialized;

    public static void Initialize()
    {
        lock (Gate)
        {
            if (_initialized)
            {
                return;
            }

            HookRegistry.RegisterAll(ModConstants.ModId);

            _initialized = true;
            ModLog.Info("Initialization completed.");
        }
    }
}
