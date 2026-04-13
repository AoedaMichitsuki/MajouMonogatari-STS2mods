using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MajouMonogatari_STS2mods.Characters.Cecily.Relics;
using MajouMonogatari_STS2mods.Shared.Core;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MajouMonogatari_STS2mods.Shared.Rules;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MajouMonogatari_STS2mods.Shared.Hooks;

public static class HookRegistry
{
    private static readonly object Gate = new();
    private static bool _registered;

    public static void RegisterAll(string modId)
    {
        lock (Gate)
        {
            if (_registered)
            {
                return;
            }

            var hookType = Type.GetType("MegaCrit.Sts2.Core.Hooks.Hook, sts2");
            if (hookType == null)
            {
                ModLog.Error("Failed to find MegaCrit.Sts2.Core.Hooks.Hook.");
                return;
            }

            var harmony = new Harmony($"{modId}.hook-registry");
            PatchByName(harmony, hookType, "ShouldPlay", nameof(ShouldPlayPostfix));
            PatchByName(harmony, hookType, "BeforeCardPlayed", nameof(BeforeCardPlayedPostfix));
            PatchByName(harmony, hookType, "AfterCardPlayed", nameof(AfterCardPlayedPostfix));
            PatchByName(harmony, hookType, "AfterBlockGained", nameof(AfterBlockGainedPostfix));

            _registered = true;
            ModLog.Info("Hook registry patched.");
        }
    }

    private static void PatchByName(Harmony harmony, Type hookType, string hookMethodName, string postfixName)
    {
        var postfix = AccessTools.Method(typeof(HookRegistry), postfixName);
        if (postfix == null)
        {
            ModLog.Error($"Missing postfix method: {postfixName}");
            return;
        }

        var targetMethods = hookType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => string.Equals(method.Name, hookMethodName, StringComparison.Ordinal))
            .ToArray();

        if (targetMethods.Length == 0)
        {
            ModLog.Warn($"Missing hook method: {hookMethodName}");
            return;
        }

        foreach (var method in targetMethods)
        {
            harmony.Patch(method, postfix: new HarmonyMethod(postfix));
        }
    }

    private static void ShouldPlayPostfix(
        CombatState combatState,
        CardModel card,
        ref AbstractModel preventer,
        AutoPlayType autoPlayType,
        ref bool __result)
    {
        if (!__result)
        {
            return;
        }

        if (BreezePlayRule.ShouldBlockPlay(card, ref preventer))
        {
            __result = false;
        }
    }

    private static void BeforeCardPlayedPostfix(CombatState combatState, CardPlay cardPlay)
    {
        FlowRuntimeState.CaptureFromHand(cardPlay.Card);
    }

    private static void AfterCardPlayedPostfix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FlowRuntimeState.Clear(cardPlay.Card);
    }

    private static void AfterBlockGainedPostfix(
        CombatState combatState,
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel cardSource)
    {
        if (creature?.Player == null || amount <= 0m)
        {
            return;
        }

        var player = creature.Player;
        if (!ReferenceEquals(player.Creature, creature))
        {
            return;
        }

        var relic = player.GetRelic<BornMagicWindRelic>();
        if (relic == null)
        {
            return;
        }

        relic.Flash();
        BreezeService.Gain(creature, 1, creature, cardSource).GetAwaiter().GetResult();
    }
}
