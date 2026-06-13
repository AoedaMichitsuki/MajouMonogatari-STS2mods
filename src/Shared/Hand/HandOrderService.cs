using System;
using System.Collections.Generic;
using System.Linq;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MajouMonogatari_STS2mods.Shared.Hand;

public static class HandOrderService
{
    public static event Action<ShiftResult> CardShifted;
    public static event Action<IReadOnlyDictionary<CardModel, int>, IReadOnlyDictionary<CardModel, int>> HandOrderChanged;

    public static ShiftResult Shift(CardModel card, ShiftDirection direction, int steps = 1, bool wrap = false)
    {
        if (card?.Pile == null || card.Pile.Type != PileType.Hand || steps <= 0)
        {
            return CreateNoMoveResult(card);
        }

        var pile = card.Pile;
        var cards = pile.Cards.Where(c => c != null).ToList();
        var originalIndex = IndexOf(cards, card);
        if (originalIndex < 0 || cards.Count <= 1)
        {
            return CreateNoMoveResult(card);
        }

        var before = SnapshotIndices(cards);
        var delta = direction == ShiftDirection.Left ? -steps : steps;
        var targetIndex = wrap
            ? Mod(originalIndex + delta, cards.Count)
            : Math.Clamp(originalIndex + delta, 0, cards.Count - 1);

        if (targetIndex == originalIndex)
        {
            return new ShiftResult(card, false, originalIndex, originalIndex, 0, before, before);
        }

        MoveWithinPile(pile, card, targetIndex);

        var afterCards = pile.Cards.Where(c => c != null).ToList();
        var after = SnapshotIndices(afterCards);
        var newIndex = IndexOf(afterCards, card);
        var primaryStepsMoved = wrap ? steps : Math.Abs(targetIndex - originalIndex);
        var result = new ShiftResult(card, true, originalIndex, newIndex, primaryStepsMoved, before, after);

        FlowRuntimeState.RefreshFromHand(card.CombatState);
        RefreshLocalHandLayout(pile);
        HandOrderChanged?.Invoke(before, after);
        CardShifted?.Invoke(result);
        return result;
    }

    public static bool MoveToIndex(CardModel card, int targetIndex)
    {
        if (card?.Pile == null || card.Pile.Type != PileType.Hand)
        {
            return false;
        }

        var pile = card.Pile;
        var cards = pile.Cards.Where(c => c != null).ToList();
        var originalIndex = IndexOf(cards, card);
        if (originalIndex < 0 || cards.Count <= 1)
        {
            return false;
        }

        targetIndex = Math.Clamp(targetIndex, 0, cards.Count - 1);
        if (targetIndex == originalIndex)
        {
            return false;
        }

        var before = SnapshotIndices(cards);
        MoveWithinPile(pile, card, targetIndex);
        var after = SnapshotIndices(pile.Cards.Where(c => c != null).ToList());

        FlowRuntimeState.RefreshFromHand(card.CombatState);
        RefreshLocalHandLayout(pile);
        HandOrderChanged?.Invoke(before, after);
        return true;
    }

    public static bool MoveToLeftmost(CardModel card)
    {
        return MoveToIndex(card, 0);
    }

    public static bool MoveToRightmost(CardModel card)
    {
        var hand = card?.Pile?.Cards;
        return hand != null && MoveToIndex(card, hand.Count - 1);
    }

    public static bool Swap(CardModel first, CardModel second)
    {
        if (first == null || second == null || first.Pile == null || !ReferenceEquals(first.Pile, second.Pile) ||
            first.Pile.Type != PileType.Hand)
        {
            return false;
        }

        var pile = first.Pile;
        var cards = pile.Cards.Where(c => c != null).ToList();
        var firstIndex = IndexOf(cards, first);
        var secondIndex = IndexOf(cards, second);
        if (firstIndex < 0 || secondIndex < 0 || firstIndex == secondIndex)
        {
            return false;
        }

        var before = SnapshotIndices(cards);
        var ordered = cards.ToList();
        ordered[firstIndex] = second;
        ordered[secondIndex] = first;
        ReplacePileOrder(pile, ordered);
        var after = SnapshotIndices(ordered);

        FlowRuntimeState.RefreshFromHand(first.CombatState);
        RefreshLocalHandLayout(pile);
        HandOrderChanged?.Invoke(before, after);
        return true;
    }

    public static bool Sort(CardPile hand, Comparison<CardModel> comparison)
    {
        if (hand == null || hand.Type != PileType.Hand || comparison == null)
        {
            return false;
        }

        var cards = hand.Cards.Where(c => c != null).ToList();
        if (cards.Count <= 1)
        {
            return false;
        }

        var before = SnapshotIndices(cards);
        var sorted = cards.ToList();
        sorted.Sort(comparison);
        if (sorted.SequenceEqual(cards, ReferenceEqualityComparer.Instance))
        {
            return false;
        }

        ReplacePileOrder(hand, sorted);
        var after = SnapshotIndices(sorted);
        FlowRuntimeState.RefreshFromHand(sorted[0].CombatState);
        RefreshLocalHandLayout(hand);
        HandOrderChanged?.Invoke(before, after);
        return true;
    }

    private static void MoveWithinPile(CardPile pile, CardModel card, int targetIndex)
    {
        pile.RemoveInternal(card, silent: true);
        if (targetIndex >= pile.Cards.Count)
        {
            pile.AddInternal(card, -1, silent: true);
        }
        else
        {
            pile.AddInternal(card, targetIndex, silent: true);
        }

        pile.InvokeContentsChanged();
        pile.InvokeCardAddFinished();
        pile.InvokeCardRemoveFinished();
    }

    private static void ReplacePileOrder(CardPile pile, IReadOnlyList<CardModel> ordered)
    {
        foreach (var card in pile.Cards.ToList())
        {
            pile.RemoveInternal(card, silent: true);
        }

        for (var i = 0; i < ordered.Count; i++)
        {
            pile.AddInternal(ordered[i], i, silent: true);
        }

        pile.InvokeContentsChanged();
        pile.InvokeCardAddFinished();
        pile.InvokeCardRemoveFinished();
    }

    private static ShiftResult CreateNoMoveResult(CardModel card)
    {
        var indices = card?.Pile?.Cards == null
            ? new Dictionary<CardModel, int>(ReferenceEqualityComparer.Instance)
            : SnapshotIndices(card.Pile.Cards.Where(c => c != null).ToList());
        var index = card == null ? -1 : indices.TryGetValue(card, out var foundIndex) ? foundIndex : -1;
        return new ShiftResult(card, false, index, index, 0, indices, indices);
    }

    private static Dictionary<CardModel, int> SnapshotIndices(IReadOnlyList<CardModel> cards)
    {
        var result = new Dictionary<CardModel, int>(ReferenceEqualityComparer.Instance);
        for (var i = 0; i < cards.Count; i++)
        {
            result[cards[i]] = i;
        }

        return result;
    }

    private static int IndexOf(IReadOnlyList<CardModel> cards, CardModel card)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            if (ReferenceEquals(cards[i], card))
            {
                return i;
            }
        }

        return -1;
    }

    private static int Mod(int value, int divisor)
    {
        var result = value % divisor;
        return result < 0 ? result + divisor : result;
    }

    private static void RefreshLocalHandLayout(CardPile handPile)
    {
        var hand = NPlayerHand.Instance;
        if (hand == null || handPile == null)
        {
            return;
        }

        var container = hand.CardHolderContainer;
        var visualIndex = 0;
        foreach (var card in handPile.Cards.Where(c => c != null))
        {
            if (hand.GetCardHolder(card) is not NHandCardHolder holder ||
                holder.GetParent() != container ||
                !holder.Visible)
            {
                continue;
            }

            container.MoveChild(holder, visualIndex);
            visualIndex++;
        }

        hand.ForceRefreshCardIndices();
    }
}
