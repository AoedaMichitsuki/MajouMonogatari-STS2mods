using Godot;
using MajouMonogatari_STS2mods.Shared.Core;

namespace MajouMonogatari_STS2mods.Shared.ArtController;

/// <summary>
/// 美术资源清单检查器。
/// 用途：
/// - 在开发期快速提示缺失资源。
/// - 不阻断初始化，只做告警，保证程序可继续运行。
/// </summary>
public static class ArtManifestReporter
{
    public static void ReportMissing(IArtManifestProvider provider)
    {
        foreach (var path in provider.GetRequiredAssetPaths())
        {
            if (ResourceLoader.Exists(path))
            {
                continue;
            }

            ModLog.Warn($"Missing art asset: {path}");
        }
    }
}
