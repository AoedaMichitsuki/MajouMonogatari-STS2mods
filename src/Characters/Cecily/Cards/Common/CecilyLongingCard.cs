using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Keywords.Chant;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Common;

[CustomID(CecilyIds.LongingCard)]
public class CecilyLongingCard() : CecilyCard(1, CardType.Skill, CardRarity.Common, TargetType.Self), IChantCard, IFlowCard
{
    private const string FlowBreezeGainVarName = "FlowBreezeGain";

    public bool HasFlowLeft => false;
    public bool HasFlowRight => true;
    public bool HasFlowVain => false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(FlowBreezeGainVarName, 5)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public async Task ResolveChant(PlayerChoiceContext choiceContext, ChantResolutionContext chantContext)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature == null || chantContext.FlowSnapshot?.IsRightmost != true)
        {
            return;
        }

        if (!DynamicVars.TryGetValue(FlowBreezeGainVarName, out var flowBreezeGainVar))
        {
            return;
        }

        await BreezeService.Gain(ownerCreature, flowBreezeGainVar.IntValue, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
