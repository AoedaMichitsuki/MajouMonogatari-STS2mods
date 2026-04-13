using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Powers;
using MajouMonogatari_STS2mods.Characters.Cecily;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Powers;

/// <summary>
/// 微风资源的承载 Power。
/// 设计为 Counter 型 Buff：
/// - Counter：作为数值资源显示。
/// - Buff：正向资源，不走负面逻辑。
/// - AllowNegative=false：保证不会跌到负数。
/// </summary>
[CustomID(CecilyIds.BreezePower)]
public sealed class BreezePower : CecilyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
}
