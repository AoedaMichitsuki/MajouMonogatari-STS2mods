using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Keywords.Locked;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Special;

[CustomID(CecilyIds.VacuoCard)]
public class CecilyVacuoCard() : CecilyCard(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        CardKeyword.Eternal
    ];

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (ReferenceEquals(card, this))
        {
            LockedRuntimeState.LockForTurn(this);
        }

        return Task.CompletedTask;
    }
}
