using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Locked;

public static class LockedRuntimeState
{
    private static readonly object Gate = new();
    private static readonly HashSet<CardModel> LockedCards = new(ReferenceEqualityComparer.Instance);

    public static void ClearAll()
    {
        lock (Gate)
        {
            LockedCards.Clear();
        }
    }

    public static void LockForTurn(CardModel card)
    {
        if (card == null)
        {
            return;
        }

        lock (Gate)
        {
            LockedCards.Add(card);
        }
    }

    public static void Unlock(CardModel card)
    {
        if (card == null)
        {
            return;
        }

        lock (Gate)
        {
            LockedCards.Remove(card);
        }
    }

    public static bool IsLocked(CardModel card)
    {
        if (card == null)
        {
            return false;
        }

        lock (Gate)
        {
            return LockedCards.Contains(card);
        }
    }

    public static bool ShouldBlockPlay(CardModel card, ref AbstractModel preventer)
    {
        if (!IsLocked(card))
        {
            return false;
        }

        preventer = card;
        return true;
    }

    public static int CountLockedInHand(Player player)
    {
        var hand = player?.PlayerCombatState?.Hand?.Cards;
        if (hand == null)
        {
            return 0;
        }

        lock (Gate)
        {
            return hand.Count(card => card != null && LockedCards.Contains(card));
        }
    }

    public static void ApplyRetainForSide(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player || combatState?.Players == null)
        {
            return;
        }

        lock (Gate)
        {
            foreach (var player in combatState.Players)
            {
                var hand = player?.PlayerCombatState?.Hand?.Cards;
                if (hand == null)
                {
                    continue;
                }

                foreach (var card in hand)
                {
                    if (card != null && LockedCards.Contains(card))
                    {
                        card.GiveSingleTurnRetain();
                    }
                }
            }
        }
    }

    public static void ClearExpiredForSide(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player || combatState?.Players == null)
        {
            return;
        }

        var players = combatState.Players.Where(player => player != null).ToHashSet(ReferenceEqualityComparer.Instance);
        lock (Gate)
        {
            LockedCards.RemoveWhere(card => card?.Owner == null || players.Contains(card.Owner));
        }
    }
}
