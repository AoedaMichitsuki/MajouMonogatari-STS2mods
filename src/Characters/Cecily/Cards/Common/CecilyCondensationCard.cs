using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Keywords.Connection;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Common;

[CustomID(CecilyIds.CondensationCard)]
public class CecilyCondensationCard() : CecilyCard(1, CardType.Skill, CardRarity.Common, TargetType.Self), IConnectionCard
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
        var owner = Owner;
        var ownerCreature = Owner?.Creature;
        if (owner == null || ownerCreature == null)
        {
            return;
        }

        if (!DynamicVars.TryGetValue(BreezeGainVarName, out var breezeGainVar))
        {
            return;
        }

        await BreezeService.Gain(ownerCreature, breezeGainVar.IntValue, ownerCreature, this);

        if (!ConnectionRuntimeState.TryResolve(cardPlay, out var connection) || !connection.IsTriggered)
        {
            return;
        }

        if (!DynamicVars.TryGetValue(LinkBreezeGainVarName, out var linkBreezeGainVar))
        {
            return;
        }

        var triggerCount = ConnectionRuntimeState.ConsumeTriggerCount(owner);
        for (var i = 0; i < triggerCount; i++)
        {
            await BreezeService.Gain(ownerCreature, linkBreezeGainVar.IntValue, ownerCreature, this);
        }
    }
}
