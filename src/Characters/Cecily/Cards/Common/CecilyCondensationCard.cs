using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Common;

[CustomID(CecilyIds.CondensationCard)]
public class CecilyCondensationCard() : CecilyCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    private const string BreezeGainVarName = "BreezeGain";
    private const string LinkBreezeGainVarName = "LinkBreezeGain";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(BreezeGainVarName, 3),
        new IntVar(LinkBreezeGainVarName, 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature == null)
        {
            return;
        }

        if (!DynamicVars.TryGetValue(BreezeGainVarName, out var breezeGainVar))
        {
            return;
        }

        await BreezeService.Gain(ownerCreature, breezeGainVar.IntValue, ownerCreature, this);

        if (!IsLinkTriggered(cardPlay))
        {
            return;
        }

        if (!DynamicVars.TryGetValue(LinkBreezeGainVarName, out var linkBreezeGainVar))
        {
            return;
        }

        await BreezeService.Gain(ownerCreature, linkBreezeGainVar.IntValue, ownerCreature, this);
    }

    private static bool IsLinkTriggered(CardPlay cardPlay)
    {
        var card = cardPlay?.Card;
        var playPileCards = card?.Owner?.PlayerCombatState?.PlayPile?.Cards;
        if (card == null || playPileCards == null || playPileCards.Count == 0)
        {
            return false;
        }

        CardModel previousCard = null;
        var skippedCurrent = false;

        for (var i = playPileCards.Count - 1; i >= 0; i--)
        {
            var candidate = playPileCards[i];
            if (candidate == null)
            {
                continue;
            }

            if (!skippedCurrent && ReferenceEquals(candidate, card))
            {
                skippedCurrent = true;
                continue;
            }

            previousCard = candidate;
            break;
        }

        if (!skippedCurrent)
        {
            previousCard = playPileCards.LastOrDefault();
        }

        return previousCard != null && previousCard.Type == card.Type;
    }
}
