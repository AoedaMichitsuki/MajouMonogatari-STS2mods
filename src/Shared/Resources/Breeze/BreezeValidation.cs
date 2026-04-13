using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MajouMonogatari_STS2mods.Shared.Resources.Breeze;

/// <summary>
/// 微风判定工具。
/// 将“是否足够打牌”逻辑与具体规则解耦。
/// </summary>
public static class BreezeValidation
{
    public static bool HasEnough(Creature creature, int required)
    {
        return BreezeService.CanSpend(creature, required);
    }
}
