using BaseLib.Abstracts;
using MajouMonogatari_STS2mods.Shared.Art;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Powers;

public abstract class CecilyPower : CustomPowerModel
{
    public override string CustomPackedIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetPowerSmallIconPath(Id.Entry), base.CustomPackedIconPath);

    public override string CustomBigIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetPowerBigIconPath(Id.Entry), base.CustomBigIconPath);

    public override string CustomBigBetaIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetPowerBigIconPath(Id.Entry), base.CustomBigBetaIconPath);
}
