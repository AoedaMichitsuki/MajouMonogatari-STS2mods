using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MajouMonogatari_STS2mods.Shared.Art;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Relics;

[Pool(typeof(CecilyRelicPool))]
public abstract class CecilyRelic : CustomRelicModel
{
    public override string PackedIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetRelicIconPath(Id.Entry), base.PackedIconPath);

    protected override string PackedIconOutlinePath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetRelicOutlineIconPath(Id.Entry), base.PackedIconOutlinePath);

    protected override string BigIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetBigRelicIconPath(Id.Entry), base.BigIconPath);
}
