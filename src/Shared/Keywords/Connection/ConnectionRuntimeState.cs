using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Connection;

public static class ConnectionRuntimeState
{
    private sealed class PlayerState
    {
        public readonly List<CardModel> PlayedThisTurn = [];
        public bool AnyPreviousCardMode;
        public int TriggeredThisTurn;
        public int BonusTriggers;
    }

    private static readonly object Gate = new();
    private static readonly Dictionary<Player, PlayerState> States = new(ReferenceEqualityComparer.Instance);
    private static readonly Dictionary<CardPlay, ConnectionSnapshot> SnapshotByCardPlay = new(ReferenceEqualityComparer.Instance);

    public static void ClearAll()
    {
        lock (Gate)
        {
            States.Clear();
            SnapshotByCardPlay.Clear();
        }
    }

    public static void BeginTurn(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player || combatState?.Players == null)
        {
            return;
        }

        lock (Gate)
        {
            foreach (var player in combatState.Players)
            {
                if (player == null)
                {
                    continue;
                }

                var state = GetOrCreateStateLocked(player);
                state.PlayedThisTurn.Clear();
                state.TriggeredThisTurn = 0;
                state.BonusTriggers = 0;
            }

            SnapshotByCardPlay.Clear();
        }
    }

    public static void CaptureForPlay(CardPlay cardPlay)
    {
        var card = cardPlay?.Card;
        var player = card?.Owner;
        if (card == null || player == null)
        {
            return;
        }

        var snapshot = EvaluateLocked(player, card);
        lock (Gate)
        {
            SnapshotByCardPlay[cardPlay] = snapshot;
        }
    }

    public static void RecordPlayed(CardPlay cardPlay)
    {
        var card = cardPlay?.Card;
        var player = card?.Owner;
        if (card == null || player == null)
        {
            return;
        }

        lock (Gate)
        {
            GetOrCreateStateLocked(player).PlayedThisTurn.Add(card);
        }
    }

    public static bool TryResolve(CardPlay cardPlay, out ConnectionSnapshot snapshot)
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

        var card = cardPlay?.Card;
        if (card?.Owner != null)
        {
            snapshot = Evaluate(card.Owner, card);
            return true;
        }

        snapshot = default;
        return false;
    }

    public static ConnectionSnapshot Evaluate(Player player, CardModel card)
    {
        if (player == null || card == null)
        {
            return default;
        }

        lock (Gate)
        {
            return EvaluateLocked(player, card);
        }
    }

    public static bool ShouldGlow(CardModel card)
    {
        if (card is not IConnectionCard || card.Owner == null)
        {
            return false;
        }

        return Evaluate(card.Owner, card).IsTriggered;
    }

    public static int GetTriggeredThisTurn(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        lock (Gate)
        {
            return States.TryGetValue(player, out var state) ? state.TriggeredThisTurn : 0;
        }
    }

    public static int ConsumeTriggerCount(Player player)
    {
        if (player == null)
        {
            return 1;
        }

        lock (Gate)
        {
            var state = GetOrCreateStateLocked(player);
            var count = Math.Max(1, 1 + state.BonusTriggers);
            state.BonusTriggers = 0;
            state.TriggeredThisTurn += count;
            return count;
        }
    }

    public static void AddBonusTriggers(Player player, int amount)
    {
        if (player == null || amount <= 0)
        {
            return;
        }

        lock (Gate)
        {
            GetOrCreateStateLocked(player).BonusTriggers += amount;
        }
    }

    public static void SetAnyPreviousCardMode(Player player, bool enabled)
    {
        if (player == null)
        {
            return;
        }

        lock (Gate)
        {
            GetOrCreateStateLocked(player).AnyPreviousCardMode = enabled;
        }
    }

    private static ConnectionSnapshot EvaluateLocked(Player player, CardModel card)
    {
        if (!States.TryGetValue(player, out var state) || state.PlayedThisTurn.Count == 0)
        {
            return new ConnectionSnapshot(false, null, null);
        }

        CardModel matchedCard;
        if (state.AnyPreviousCardMode)
        {
            matchedCard = state.PlayedThisTurn.LastOrDefault(played => IsSameType(played, card));
        }
        else
        {
            matchedCard = state.PlayedThisTurn.LastOrDefault();
            if (!IsSameType(matchedCard, card))
            {
                matchedCard = null;
            }
        }

        return new ConnectionSnapshot(matchedCard != null, matchedCard, matchedCard?.Type);
    }

    private static bool IsSameType(CardModel played, CardModel current)
    {
        return played != null && current != null && played.Type == current.Type;
    }

    private static PlayerState GetOrCreateStateLocked(Player player)
    {
        if (!States.TryGetValue(player, out var state))
        {
            state = new PlayerState();
            States[player] = state;
        }

        return state;
    }
}
