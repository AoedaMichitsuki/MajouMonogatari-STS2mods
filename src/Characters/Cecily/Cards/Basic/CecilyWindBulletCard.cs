using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;

[CustomID(CecilyIds.WindBulletCard)]
public class CecilyWindBulletCard() : CecilyCard(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy), IBreezeCostCard
{
    private const string FlowGainVarName = "FlowGain";

    public int BreezeCost => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new IntVar(FlowGainVarName, 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature == null || cardPlay.Target == null)
        {
            return;
        }

        var breezeBeforeSpend = BreezeService.GetCurrent(ownerCreature);
        if (!await BreezeService.Spend(ownerCreature, BreezeCost, ownerCreature, this))
        {
            return;
        }

        var totalDamage = DynamicVars.Damage.IntValue + breezeBeforeSpend;
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, totalDamage, ValueProp.Move, ownerCreature, this);

        if (!FlowRuntimeState.TryResolve(cardPlay?.Card ?? this, cardPlay, out var flowSnapshot) || !flowSnapshot.IsLeftmost)
        {
            return;
        }

        if (!DynamicVars.TryGetValue(FlowGainVarName, out var flowGainVar))
        {
            return;
        }

        await BreezeService.Gain(ownerCreature, flowGainVar.IntValue, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        if (DynamicVars.TryGetValue(FlowGainVarName, out var flowGainVar))
        {
            flowGainVar.UpgradeValueBy(2m);
        }
    }
}
