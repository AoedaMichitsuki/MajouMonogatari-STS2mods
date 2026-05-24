using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Connection;

public readonly struct ConnectionSnapshot
{
    public ConnectionSnapshot(bool isTriggered, CardModel matchedCard, CardType? matchedType)
    {
        IsTriggered = isTriggered;
        MatchedCard = matchedCard;
        MatchedType = matchedType;
    }

    public bool IsTriggered { get; }
    public CardModel MatchedCard { get; }
    public CardType? MatchedType { get; }
}
