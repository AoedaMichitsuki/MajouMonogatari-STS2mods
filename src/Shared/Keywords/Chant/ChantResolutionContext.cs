using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Chant;

public sealed class ChantResolutionContext
{
    public ChantResolutionContext(CardModel card, Creature target, FlowSnapshot? flowSnapshot)
    {
        Card = card;
        Target = target;
        FlowSnapshot = flowSnapshot;
    }

    public CardModel Card { get; }
    public Creature Target { get; }
    public FlowSnapshot? FlowSnapshot { get; }
    public PileType ResultPileType { get; set; } = PileType.None;
    public CardPilePosition ResultPilePosition { get; set; } = CardPilePosition.Bottom;
    public bool SuppressDefaultMove { get; set; }
}
