using BaseLib.Abstracts;
using MajouMonogatari_STS2mods.Shared.ArtController;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Powers;

/// <summary>
/// 希赛莉 Power 基类。
/// 目标：
/// - 统一接入图标资源路径策略。
/// - 后续扩展共通行为（例如统一 tooltip 或调试标记）时有集中入口。
/// </summary>
public abstract class CecilyPower : CustomPowerModel
{
    public override string CustomPackedIconPath => CecilyArtProvider.Instance.GetPowerSmallIconPath(Id.Entry);
    public override string CustomBigIconPath => CecilyArtProvider.Instance.GetPowerBigIconPath(Id.Entry);
    public override string CustomBigBetaIconPath => CecilyArtProvider.Instance.GetPowerBigBetaIconPath(Id.Entry);
}
