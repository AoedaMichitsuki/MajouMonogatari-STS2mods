using MegaCrit.Sts2.Core.Models;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;

namespace MajouMonogatari_STS2mods.Shared.Rules;

/// <summary>
/// 微风出牌规则：
/// - 只关心“是否允许打出”，不执行资源扣减。
/// - 资源扣减由卡牌 OnPlay 内执行，保证规则判定与效果执行解耦。
/// </summary>
public static class BreezePlayRule
{
    /// <summary>
    /// 若返回 true，表示应拦截本次出牌。
    /// preventer 会被设置为触发拦截的卡牌本身，用于原因回溯。
    /// </summary>
    public static bool ShouldBlockPlay(CardModel card, ref AbstractModel preventer)
    {
        if (card is not IBreezeCostCard breezeCard)
        {
            return false;
        }

        if (breezeCard.BreezeCost <= 0)
        {
            return false;
        }

        var ownerCreature = card.Owner?.Creature;
        if (ownerCreature == null)
        {
            return false;
        }

        if (BreezeService.CanSpend(ownerCreature, breezeCard.BreezeCost))
        {
            return false;
        }

        preventer = card;
        return true;
    }
}
