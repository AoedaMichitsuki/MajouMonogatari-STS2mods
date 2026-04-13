using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Rules;

public static class BreezePlayRule
{
    public static bool ShouldBlockPlay(CardModel card, ref AbstractModel preventer)
    {
        if (card is not IBreezeCostCard breezeCostCard)
        {
            return false;
        }

        if (breezeCostCard.BreezeCost <= 0)
        {
            return false;
        }

        var ownerCreature = card.Owner?.Creature;
        if (ownerCreature == null)
        {
            return false;
        }

        if (BreezeService.CanSpend(ownerCreature, breezeCostCard.BreezeCost))
        {
            return false;
        }

        preventer = card;
        return true;
    }
}
