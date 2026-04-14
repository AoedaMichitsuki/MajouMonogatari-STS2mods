using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Rare;

[CustomID(CecilyIds.BlossomWayCard)]
public class CecilyBlossomWayCard() : CecilyCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(2, ValueProp.Move),
        new RepeatVar(3)
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
            var drawnCards = await CardPileCmd.Draw(choiceContext, 1, owner, false);
            var firstDrawn = drawnCards?.FirstOrDefault();
            firstDrawn?.SetToFreeThisTurn();
        }

        if (!flowSnapshot.IsRightmost || owner.PlayerCombatState?.Hand?.Cards == null || owner.PlayerCombatState.Hand.Cards.Count <= 0)
        {
            return;
        }

        var exhaustPrefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1)
        {
            Cancelable = false
        };

        var selectedCards = await CardSelectCmd.FromHand(choiceContext, owner, exhaustPrefs, card => true, this);
        var selected = selectedCards?.FirstOrDefault();
        if (selected != null)
        {
            await CardCmd.Exhaust(choiceContext, selected, false, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
