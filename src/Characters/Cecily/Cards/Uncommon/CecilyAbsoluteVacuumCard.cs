using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards.Special;
using MajouMonogatari_STS2mods.Characters.Cecily.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Uncommon;

[CustomID(CecilyIds.AbsoluteVacuumCard)]
public class CecilyAbsoluteVacuumCard() : CecilyCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    private const int VacuoCardsToAdd = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner?.Creature == null)
        {
            return;
        }

        await CommonActions.ApplySelf<AbsoluteVacuumPower>(this);

        for (var i = 0; i < VacuoCardsToAdd; i++)
        {
            var vacuo = CombatState.CreateCard<CecilyVacuoCard>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(vacuo, PileType.Hand, owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
