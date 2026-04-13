using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MajouMonogatari_STS2mods.Shared.Art;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Relics;

[Pool(typeof(CecilyRelicPool))]
public abstract class CecilyRelic : CustomRelicModel
{
    public override string PackedIconPath => CecilyArtProvider.Instance.GetRelicIconPath(Id.Entry);

    protected override string PackedIconOutlinePath => CecilyArtProvider.Instance.GetRelicOutlineIconPath(Id.Entry);

    protected override string BigIconPath => CecilyArtProvider.Instance.GetBigRelicIconPath(Id.Entry);
}
