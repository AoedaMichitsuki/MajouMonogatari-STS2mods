using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Flow;

public readonly struct FlowSnapshot
{
    public FlowSnapshot(bool isLeftmost, bool isRightmost, bool isOnlyCard)
    {
        IsLeftmost = isLeftmost;
        IsRightmost = isRightmost;
        IsOnlyCard = isOnlyCard;
    }

    public bool IsLeftmost { get; }
    public bool IsRightmost { get; }
    public bool IsOnlyCard { get; }
}

public static class FlowRuntimeState
{
    private static readonly object Gate = new();
    private static readonly Dictionary<CardModel, FlowSnapshot> SnapshotByCard = new(ReferenceEqualityComparer.Instance);

    public static void CaptureFromHand(CardModel card)
    {
        if (card == null)
        {
            return;
        }

        var handCards = card.Owner?.PlayerCombatState?.Hand?.Cards;
        if (handCards == null)
        {
            Clear(card);
            return;
        }

        var index = -1;
        for (var i = 0; i < handCards.Count; i++)
        {
            if (!ReferenceEquals(handCards[i], card))
            {
                continue;
            }

            index = i;
            break;
        }

        if (index < 0)
        {
            Clear(card);
            return;
        }

        var count = handCards.Count;
        var snapshot = new FlowSnapshot(
            isLeftmost: index == 0,
            isRightmost: index == count - 1,
            isOnlyCard: count == 1);

        lock (Gate)
        {
            SnapshotByCard[card] = snapshot;
        }
    }

    public static bool TryGet(CardModel card, out FlowSnapshot snapshot)
    {
        if (card != null)
        {
            lock (Gate)
            {
                if (SnapshotByCard.TryGetValue(card, out snapshot))
                {
                    return true;
                }
            }
        }

        snapshot = default;
        return false;
    }

    public static void Clear(CardModel card)
    {
        if (card == null)
        {
            return;
        }

        lock (Gate)
        {
            SnapshotByCard.Remove(card);
        }
    }
}
