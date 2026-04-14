using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Resources.Breeze;

public static class BreezeService
{
    private sealed class BreezeState
    {
        public int Amount;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Creature, BreezeState> States = new();

    private static BreezeState GetOrCreateState(Creature creature)
    {
        return States.GetValue(creature, static _ => new BreezeState());
    }

    public static int GetCurrent(Creature creature)
    {
        if (creature == null)
        {
            return 0;
        }

        return States.TryGetValue(creature, out var state) ? Math.Max(0, state.Amount) : 0;
    }

    public static bool CanSpend(Creature creature, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return GetCurrent(creature) >= amount;
    }

    public static Task Gain(Creature creature, int amount, Creature applier, CardModel sourceCard, bool silent = false)
    {
        if (creature == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        var state = GetOrCreateState(creature);
        checked
        {
            state.Amount += amount;
        }

        return Task.CompletedTask;
    }

    public static async Task<bool> Spend(Creature creature, int amount, Creature applier, CardModel sourceCard, bool silent = false)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (creature == null || !CanSpend(creature, amount))
        {
            return false;
        }

        var state = GetOrCreateState(creature);
        state.Amount = Math.Max(0, state.Amount - amount);
        await Task.CompletedTask;
        return true;
    }

    public static void Reset(Creature creature)
    {
        if (creature == null)
        {
            return;
        }

        if (States.TryGetValue(creature, out var state))
        {
            state.Amount = 0;
        }
    }

    public static void ResetForCombat(CombatState combatState)
    {
        if (combatState?.Players == null)
        {
            return;
        }

        foreach (var player in combatState.Players)
        {
            Reset(player?.Creature);
        }
    }
}
