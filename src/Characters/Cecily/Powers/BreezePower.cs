using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Powers;

[CustomID(CecilyIds.BreezePower)]
public class BreezePower : CecilyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
}
