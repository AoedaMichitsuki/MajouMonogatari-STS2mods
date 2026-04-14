using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Uncommon;

[CustomID(CecilyIds.TrapCard)]
public class CecilyTrapCard() : CecilyCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private const string ThornsVarName = "Thorns";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(ThornsVarName, 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ownerCreature = Owner?.Creature;
        if (ownerCreature == null)
        {
            return;
        }

        if (!DynamicVars.TryGetValue(ThornsVarName, out var thornsVar))
        {
            return;
        }

        await PowerCmd.Apply<ThornsPower>(ownerCreature, thornsVar.IntValue, ownerCreature, this, false);
    }

    protected override void OnUpgrade()
    {
        if (DynamicVars.TryGetValue(ThornsVarName, out var thornsVar))
        {
            thornsVar.UpgradeValueBy(1m);
        }
    }
}
