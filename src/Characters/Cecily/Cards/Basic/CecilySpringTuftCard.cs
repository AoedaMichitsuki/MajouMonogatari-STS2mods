using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;

[CustomID(CecilyIds.SpringTuftCard)]
public class CecilySpringTuftCard() : CecilyCard(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(2, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        var ownerCreature = owner?.Creature;
        if (owner == null || ownerCreature == null)
        {
            return;
        }

        var repeatCount = DynamicVars.Repeat.IntValue;
        if (repeatCount < 1)
        {
            repeatCount = 1;
        }

        for (var i = 0; i < repeatCount; i++)
        {
            await CreatureCmd.GainBlock(ownerCreature, DynamicVars.Block, cardPlay, false);
        }

        if (!FlowRuntimeState.TryResolve(cardPlay?.Card ?? this, cardPlay, out var flowSnapshot))
        {
            return;
        }

        if (flowSnapshot.IsLeftmost)
        {
            await CardPileCmd.Draw(choiceContext, 1, owner, false);
        }

        if (!flowSnapshot.IsRightmost || owner.PlayerCombatState.Hand.Cards.Count <= 0)
        {
            return;
        }

        var discardPrefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1)
        {
            Cancelable = false
        };

        var selectedCards = await CardSelectCmd.FromHand(choiceContext, owner, discardPrefs, card => true, this);
        var selected = selectedCards?.FirstOrDefault();
        if (selected != null)
        {
            await CardCmd.Discard(choiceContext, selected);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
