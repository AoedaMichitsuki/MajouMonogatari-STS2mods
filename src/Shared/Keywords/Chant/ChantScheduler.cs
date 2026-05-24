using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MajouMonogatari_STS2mods.Shared.Keywords.Locked;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Chant;

public static class ChantScheduler
{
    private sealed class ChantEntry
    {
        public CardModel Card;
        public Player Owner;
        public Creature Target;
        public bool ReadyForNextTurn;
        public FlowSnapshot? EndTurnFlowSnapshot;
    }

    private static readonly object Gate = new();
    private static readonly Dictionary<CardModel, ChantEntry> Entries = new(ReferenceEqualityComparer.Instance);
    private static readonly HashSet<CardModel> ResolvingCards = new(ReferenceEqualityComparer.Instance);

    public static void ClearAll()
    {
        lock (Gate)
        {
            Entries.Clear();
            ResolvingCards.Clear();
        }
    }

    public static bool IsResolving(CardModel card)
    {
        if (card == null)
        {
            return false;
        }

        lock (Gate)
        {
            return ResolvingCards.Contains(card);
        }
    }

    public static bool ShouldReturnToHandOnPlay(CardModel card, bool isAutoPlay)
    {
        return card is IChantCard && !isAutoPlay && !IsResolving(card);
    }

    public static void StageFromPlay(CardPlay cardPlay)
    {
        var card = cardPlay?.Card;
        if (card is not IChantCard || cardPlay.IsAutoPlay || IsResolving(card) || card.Owner == null)
        {
            return;
        }

        lock (Gate)
        {
            Entries[card] = new ChantEntry
            {
                Card = card,
                Owner = card.Owner,
                Target = cardPlay.Target,
                ReadyForNextTurn = false,
                EndTurnFlowSnapshot = null
            };
        }

        LockedRuntimeState.LockForTurn(card);
    }

    public static void PrepareForNextTurn(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player || combatState?.Players == null)
        {
            return;
        }

        lock (Gate)
        {
            foreach (var entry in Entries.Values.ToList())
            {
                if (entry.Card?.Pile == null || entry.Card.Pile.Type != PileType.Hand)
                {
                    Entries.Remove(entry.Card);
                    continue;
                }

                if (!combatState.Players.Contains(entry.Owner))
                {
                    continue;
                }

                entry.EndTurnFlowSnapshot = CaptureEndTurnFlowSnapshot(entry.Card);
                entry.ReadyForNextTurn = true;
                LockedRuntimeState.LockForTurn(entry.Card);
            }
        }
    }

    public static async Task ResolveDueBeforeHandDraw(PlayerChoiceContext choiceContext, Player player)
    {
        if (player?.PlayerCombatState?.Hand?.Cards == null)
        {
            return;
        }

        List<ChantEntry> dueEntries;
        lock (Gate)
        {
            dueEntries = player.PlayerCombatState.Hand.Cards
                .Where(card => card != null && Entries.TryGetValue(card, out var entry) && entry.ReadyForNextTurn)
                .Select(card => Entries[card])
                .ToList();
        }

        foreach (var entry in dueEntries)
        {
            if (entry.Card is not IChantCard chantCard || entry.Card.Pile == null || entry.Card.Pile.Type != PileType.Hand)
            {
                Remove(entry.Card);
                continue;
            }

            var chantContext = new ChantResolutionContext(entry.Card, entry.Target, entry.EndTurnFlowSnapshot)
            {
                ResultPileType = GetDefaultResultPile(entry.Card),
                ResultPilePosition = CardPilePosition.Bottom
            };

            lock (Gate)
            {
                ResolvingCards.Add(entry.Card);
            }

            try
            {
                await chantCard.ResolveChant(choiceContext, chantContext);
            }
            finally
            {
                lock (Gate)
                {
                    ResolvingCards.Remove(entry.Card);
                }
            }

            if (!chantContext.SuppressDefaultMove)
            {
                await MoveResolvedCard(choiceContext, entry.Card, chantContext.ResultPileType, chantContext.ResultPilePosition);
            }

            Remove(entry.Card);
        }
    }

    public static async Task ResolveNow(PlayerChoiceContext choiceContext, CardModel card, Creature target = null, FlowSnapshot? flowSnapshot = null)
    {
        if (card is not IChantCard chantCard)
        {
            return;
        }

        var chantContext = new ChantResolutionContext(card, target, flowSnapshot)
        {
            ResultPileType = GetDefaultResultPile(card),
            ResultPilePosition = CardPilePosition.Bottom,
            SuppressDefaultMove = true
        };

        lock (Gate)
        {
            ResolvingCards.Add(card);
        }

        try
        {
            await chantCard.ResolveChant(choiceContext, chantContext);
        }
        finally
        {
            lock (Gate)
            {
                ResolvingCards.Remove(card);
            }
        }
    }

    private static void Remove(CardModel card)
    {
        if (card == null)
        {
            return;
        }

        lock (Gate)
        {
            Entries.Remove(card);
        }
    }

    private static FlowSnapshot? CaptureEndTurnFlowSnapshot(CardModel card)
    {
        if (FlowRuntimeState.CaptureFromHand(card) && FlowRuntimeState.TryGet(card, out var snapshot))
        {
            return snapshot;
        }

        return null;
    }

    private static PileType GetDefaultResultPile(CardModel card)
    {
        if (card == null || card.IsDupe || card.Type == CardType.Power)
        {
            return PileType.None;
        }

        if (card.ExhaustOnNextPlay || card.Keywords.Contains(CardKeyword.Exhaust))
        {
            return PileType.Exhaust;
        }

        return PileType.Discard;
    }

    private static async Task MoveResolvedCard(
        PlayerChoiceContext choiceContext,
        CardModel card,
        PileType pileType,
        CardPilePosition position)
    {
        if (card?.Pile == null || card.Pile.Type != PileType.Hand)
        {
            return;
        }

        switch (pileType)
        {
            case PileType.None:
                await CardPileCmd.RemoveFromCombat(card);
                break;
            case PileType.Exhaust:
                await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false);
                break;
            default:
                await CardPileCmd.Add(card, pileType, position);
                break;
        }
    }
}
