using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Hand;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Common;

[CustomID(CecilyIds.BlowCard)]
public class CecilyBlowCard() : CecilyCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    private const string ShiftVarName = "Shift";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(ShiftVarName, 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        var hand = owner?.PlayerCombatState?.Hand?.Cards;
        if (owner == null || hand == null || hand.Count <= 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1)
        {
            Cancelable = false
        };

        var selected = (await CardSelectCmd.FromHand(choiceContext, owner, prefs, card => card != null, this))
            ?.FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        var steps = DynamicVars.TryGetValue(ShiftVarName, out var shiftVar) ? shiftVar.IntValue : 1;
        HandOrderService.Shift(selected, ShiftDirection.Right, steps);
    }

    protected override void OnUpgrade()
    {
        if (DynamicVars.TryGetValue(ShiftVarName, out var shiftVar))
        {
            shiftVar.UpgradeValueBy(1m);
        }
    }
}
