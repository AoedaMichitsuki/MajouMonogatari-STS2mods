using System;
using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Context;
using MajouMonogatari_STS2mods.Shared.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
    private static readonly Dictionary<CardPlay, FlowSnapshot> SnapshotByCardPlay = new(ReferenceEqualityComparer.Instance);
    private static readonly Dictionary<string, FlowSnapshot> SnapshotByUniqueId = new(StringComparer.Ordinal);
    private static readonly PropertyInfo CardUniqueIdProperty = typeof(CardModel).GetProperty("UniqueId");

    public static void RefreshFromHand(CombatState combatState)
    {
        if (combatState == null)
        {
            return;
        }

        var player = LocalContext.GetMe(combatState);
        if (player == null)
        {
            return;
        }

        var handCards = CardPile.GetCards(player, PileType.Hand);
        if (handCards == null)
        {
            return;
        }

        lock (Gate)
        {
            SnapshotByCard.Clear();
            SnapshotByUniqueId.Clear();

            var index = 0;
            var cards = new List<CardModel>();
            foreach (var c in handCards)
            {
                if (c != null)
                {
                    cards.Add(c);
                }
            }

            var count = cards.Count;
            for (index = 0; index < count; index++)
            {
                var card = cards[index];
                var snapshot = new FlowSnapshot(
                    isLeftmost: index == 0,
                    isRightmost: index == count - 1,
                    isOnlyCard: count == 1);

                SnapshotByCard[card] = snapshot;
                if (TryGetUniqueId(card, out var uniqueId))
                {
                    SnapshotByUniqueId[uniqueId] = snapshot;
                }
            }
        }
    }

    public static bool CaptureFromHand(CardPlay cardPlay)
    {
        if (cardPlay?.Card == null)
        {
            return false;
        }

        // Preferred path: if a snapshot already exists (e.g. captured at Hand->Play removal), bind it to this CardPlay.
        if (TryGet(cardPlay.Card, out var existingSnapshot))
        {
            lock (Gate)
            {
                SnapshotByCardPlay[cardPlay] = existingSnapshot;
            }
            return true;
        }

        if (!CaptureFromHand(cardPlay.Card))
        {
            return false;
        }

        if (!TryGet(cardPlay.Card, out var snapshot))
        {
            return false;
        }

        lock (Gate)
        {
            SnapshotByCardPlay[cardPlay] = snapshot;
        }

        return true;
    }

    public static bool CaptureFromHand(CardModel card)
    {
        if (card == null)
        {
            return false;
        }

        var handCards = card.Owner?.PlayerCombatState?.Hand?.Cards;
        if (handCards == null)
        {
            Clear(card);
            return false;
        }

        var index = -1;
        CardModel resolvedHandCard = null;
        for (var i = 0; i < handCards.Count; i++)
        {
            if (!ReferenceEquals(handCards[i], card))
            {
                continue;
            }

            index = i;
            resolvedHandCard = handCards[i];
            break;
        }

        if (index < 0 && !TryFindEquivalentInHand(card, handCards, out index, out resolvedHandCard))
        {
            Clear(card);
            return false;
        }

        var count = handCards.Count;
        var snapshot = new FlowSnapshot(
            isLeftmost: index == 0,
            isRightmost: index == count - 1,
            isOnlyCard: count == 1);

        lock (Gate)
        {
            SnapshotByCard[card] = snapshot;
            if (resolvedHandCard != null)
            {
                SnapshotByCard[resolvedHandCard] = snapshot;
            }

            if (TryGetUniqueId(card, out var uniqueId))
            {
                SnapshotByUniqueId[uniqueId] = snapshot;
            }

            if (resolvedHandCard != null && TryGetUniqueId(resolvedHandCard, out var resolvedUniqueId))
            {
                SnapshotByUniqueId[resolvedUniqueId] = snapshot;
            }
        }

        return true;
    }

    public static bool CaptureFromPile(CardPile pile, CardModel card)
    {
        if (pile == null || card == null)
        {
            return false;
        }

        var cards = pile.Cards;
        if (cards == null)
        {
            return false;
        }

        var index = -1;
        for (var i = 0; i < cards.Count; i++)
        {
            if (ReferenceEquals(cards[i], card))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return false;
        }

        var count = cards.Count;
        var snapshot = new FlowSnapshot(
            isLeftmost: index == 0,
            isRightmost: index == count - 1,
            isOnlyCard: count == 1);

        lock (Gate)
        {
            SnapshotByCard[card] = snapshot;
            if (TryGetUniqueId(card, out var uniqueId))
            {
                SnapshotByUniqueId[uniqueId] = snapshot;
            }
        }

        return true;
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

                if (TryGetUniqueId(card, out var uniqueId) &&
                    SnapshotByUniqueId.TryGetValue(uniqueId, out snapshot))
                {
                    return true;
                }
            }
        }

        snapshot = default;
        return false;
    }

    public static bool TryGetOrCapture(CardModel card, out FlowSnapshot snapshot)
    {
        if (TryGet(card, out snapshot))
        {
            return true;
        }

        if (CaptureFromHand(card))
        {
            return TryGet(card, out snapshot);
        }

        snapshot = default;
        return false;
    }

    public static bool TryResolve(CardModel card, CardPlay cardPlay, out FlowSnapshot snapshot)
    {
        if (cardPlay != null)
        {
            lock (Gate)
            {
                if (SnapshotByCardPlay.TryGetValue(cardPlay, out snapshot))
                {
                    return true;
                }
            }
        }

        // Fallback: use card-level snapshot captured in ShouldPlay while the card was still in hand.
        if (TryGetOrCapture(card, out snapshot))
        {
            return true;
        }

        snapshot = default;
        ModLog.Warn(
            $"Flow snapshot unavailable for {card?.Id.Entry ?? "<null>"} " +
            $"(isAutoPlay={cardPlay?.IsAutoPlay}, playIndex={cardPlay?.PlayIndex}).");
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
            if (TryGetUniqueId(card, out var uniqueId))
            {
                SnapshotByUniqueId.Remove(uniqueId);
            }
        }
    }

    public static void Clear(CardPlay cardPlay)
    {
        if (cardPlay == null)
        {
            return;
        }

        lock (Gate)
        {
            SnapshotByCardPlay.Remove(cardPlay);
        }
    }

    public static void ClearAll()
    {
        lock (Gate)
        {
            SnapshotByCard.Clear();
            SnapshotByCardPlay.Clear();
            SnapshotByUniqueId.Clear();
        }
    }

    private static bool TryGetUniqueId(CardModel card, out string uniqueId)
    {
        uniqueId = null;
        if (card == null || CardUniqueIdProperty == null)
        {
            return false;
        }

        var raw = CardUniqueIdProperty.GetValue(card);
        var text = raw?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        uniqueId = text;
        return true;
    }

    private static bool TryFindEquivalentInHand(
        CardModel card,
        IReadOnlyList<CardModel> handCards,
        out int index,
        out CardModel resolved)
    {
        index = -1;
        resolved = null;
        if (card == null || handCards == null)
        {
            return false;
        }

        if (TryFindReference(card.DeckVersion, handCards, out index, out resolved))
        {
            return true;
        }

        if (TryFindReference(card.CloneOf, handCards, out index, out resolved))
        {
            return true;
        }

        var matchIndex = -1;
        for (var i = 0; i < handCards.Count; i++)
        {
            var handCard = handCards[i];
            if (handCard == null)
            {
                continue;
            }

            if (!ReferenceEquals(handCard.Owner, card.Owner))
            {
                continue;
            }

            if (!string.Equals(handCard.Id.Entry, card.Id.Entry, StringComparison.Ordinal))
            {
                continue;
            }

            if (matchIndex >= 0)
            {
                return false;
            }

            matchIndex = i;
        }

        if (matchIndex < 0)
        {
            return false;
        }

        index = matchIndex;
        resolved = handCards[matchIndex];
        return true;
    }

    private static bool TryFindReference(CardModel target, IReadOnlyList<CardModel> handCards, out int index, out CardModel resolved)
    {
        index = -1;
        resolved = null;
        if (target == null || handCards == null)
        {
            return false;
        }

        for (var i = 0; i < handCards.Count; i++)
        {
            if (!ReferenceEquals(handCards[i], target))
            {
                continue;
            }

            index = i;
            resolved = handCards[i];
            return true;
        }

        return false;
    }

}
