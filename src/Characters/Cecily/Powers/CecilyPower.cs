using BaseLib.Abstracts;
using MajouMonogatari_STS2mods.Shared.Art;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Powers;

public abstract class CecilyPower : CustomPowerModel
{
    public override string CustomPackedIconPath => CecilyArtProvider.Instance.GetPowerSmallIconPath(Id.Entry);
    public override string CustomBigIconPath => CecilyArtProvider.Instance.GetPowerBigIconPath(Id.Entry);
    public override string CustomBigBetaIconPath => CecilyArtProvider.Instance.GetPowerBigIconPath(Id.Entry);
}
