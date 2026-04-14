using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Characters.Cecily.Relics;
using MajouMonogatari_STS2mods.Shared.Art;
using MajouMonogatari_STS2mods.Shared.Core;
using MajouMonogatari_STS2mods.Shared.Keywords.Flow;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;
using MajouMonogatari_STS2mods.Shared.Rules;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace MajouMonogatari_STS2mods.Shared.Hooks;

public static class HookRegistry
{
    private const string BreezeCounterNodeName = "CecilyBreezeCounter";
    private const string BreezeValueLabelPath = "CecilyBreezeCounter/BreezeValue";

    private static readonly FieldInfo EnergyCounterPlayerField = AccessTools.Field(
        "MegaCrit.Sts2.Core.Nodes.Combat.NEnergyCounter:_player");

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
            PatchByName(harmony, hookType, "ShouldPlay", postfixName: nameof(ShouldPlayPostfix));
            PatchByName(harmony, hookType, "BeforeCardPlayed", prefixName: nameof(BeforeCardPlayedPrefix));
            PatchByName(harmony, hookType, "AfterCardPlayed", nameof(AfterCardPlayedPostfix));
            PatchByName(harmony, hookType, "AfterBlockGained", nameof(AfterBlockGainedPostfix));
            PatchByName(harmony, hookType, "BeforeCombatStart", nameof(BeforeCombatStartPostfix));
            PatchEnergyCounterByName(harmony, "_Ready", nameof(EnergyCounterReadyPostfix));
            PatchEnergyCounterByName(harmony, "_Process", nameof(EnergyCounterProcessPostfix));
            PatchCardPileByName(harmony, nameof(CardPile.RemoveInternal), nameof(CardPileRemoveInternalPrefix));

            _registered = true;
            ModLog.Info("Hook registry patched.");
        }
    }

    private static void PatchByName(
        Harmony harmony,
        Type hookType,
        string hookMethodName,
        string postfixName = null,
        string prefixName = null)
    {
        var postfix = string.IsNullOrWhiteSpace(postfixName) ? null : AccessTools.Method(typeof(HookRegistry), postfixName);
        var prefix = string.IsNullOrWhiteSpace(prefixName) ? null : AccessTools.Method(typeof(HookRegistry), prefixName);

        if (postfix == null && prefix == null)
        {
            ModLog.Error($"Missing patch methods for hook: {hookMethodName}");
            return;
        }

        if (!string.IsNullOrWhiteSpace(postfixName) && postfix == null)
        {
            ModLog.Error($"Missing postfix method: {postfixName}");
            return;
        }

        if (!string.IsNullOrWhiteSpace(prefixName) && prefix == null)
        {
            ModLog.Error($"Missing prefix method: {prefixName}");
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
            harmony.Patch(
                method,
                prefix: prefix == null ? null : new HarmonyMethod(prefix),
                postfix: postfix == null ? null : new HarmonyMethod(postfix));
        }
    }

    private static void PatchEnergyCounterByName(Harmony harmony, string targetMethodName, string postfixName)
    {
        var postfix = AccessTools.Method(typeof(HookRegistry), postfixName);
        if (postfix == null)
        {
            ModLog.Error($"Missing postfix method: {postfixName}");
            return;
        }

        var targetMethod = AccessTools.Method(typeof(NEnergyCounter), targetMethodName);
        if (targetMethod == null)
        {
            ModLog.Warn($"Missing NEnergyCounter method: {targetMethodName}");
            return;
        }

        harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));
    }

    private static void PatchCardPileByName(Harmony harmony, string targetMethodName, string prefixName)
    {
        var prefix = AccessTools.Method(typeof(HookRegistry), prefixName);
        if (prefix == null)
        {
            ModLog.Error($"Missing CardPile prefix method: {prefixName}");
            return;
        }

        var targetMethod = AccessTools.Method(typeof(CardPile), targetMethodName);
        if (targetMethod == null)
        {
            ModLog.Warn($"Missing CardPile method: {targetMethodName}");
            return;
        }

        harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefix));
    }

    private static void ShouldPlayPostfix(
        CombatState combatState,
        CardModel card,
        ref AbstractModel preventer,
        AutoPlayType autoPlayType,
        ref bool __result)
    {
        // Keep an up-to-date hand position snapshot during play validation.
        FlowRuntimeState.RefreshFromHand(combatState);

        if (!__result)
        {
            return;
        }

        if (BreezePlayRule.ShouldBlockPlay(card, ref preventer))
        {
            __result = false;
        }
    }

    private static void BeforeCardPlayedPrefix(CombatState combatState, CardPlay cardPlay)
    {
        if (cardPlay?.Card == null)
        {
            return;
        }

        // Snapshot by CardPlay so OnPlay always reads the exact play instance.
        FlowRuntimeState.CaptureFromHand(cardPlay);
    }

    private static void AfterCardPlayedPostfix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FlowRuntimeState.Clear(cardPlay);
        FlowRuntimeState.RefreshFromHand(combatState);
    }

    private static void CardPileRemoveInternalPrefix(CardPile __instance, CardModel card, bool silent)
    {
        if (__instance == null || card == null)
        {
            return;
        }

        if (__instance.Type != PileType.Hand)
        {
            return;
        }

        FlowRuntimeState.CaptureFromPile(__instance, card);
    }

    private static void BeforeCombatStartPostfix(object runState, CombatState combatState)
    {
        FlowRuntimeState.ClearAll();
        BreezeService.ResetForCombat(combatState);
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

    private static void EnergyCounterReadyPostfix(NEnergyCounter __instance)
    {
        if (!TryGetCecilyPlayer(__instance, out _))
        {
            return;
        }

        if (__instance.GetNodeOrNull<Control>(BreezeCounterNodeName) != null)
        {
            return;
        }

        var counter = new Control
        {
            Name = BreezeCounterNodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(60f, 2f),
            Size = new Vector2(64f, 32f)
        };

        var icon = new TextureRect
        {
            Name = "BreezeIcon",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(0f, 4f),
            Size = new Vector2(24f, 24f),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };

        var iconPath = CecilyArtProvider.Instance.GetPowerSmallIconPath(CecilyIds.BreezePower);
        if (!string.IsNullOrWhiteSpace(iconPath) && ResourceLoader.Exists(iconPath))
        {
            icon.Texture = ResourceLoader.Load<Texture2D>(iconPath);
        }

        var value = new Label
        {
            Name = "BreezeValue",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(26f, 2f),
            Size = new Vector2(36f, 24f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "0",
            Modulate = Colors.White
        };

        counter.AddChild(icon);
        counter.AddChild(value);
        __instance.AddChild(counter);
    }

    private static void EnergyCounterProcessPostfix(NEnergyCounter __instance)
    {
        if (!TryGetCecilyPlayer(__instance, out var player))
        {
            return;
        }

        var valueLabel = __instance.GetNodeOrNull<Label>(BreezeValueLabelPath);
        if (valueLabel == null)
        {
            return;
        }

        var breeze = BreezeService.GetCurrent(player.Creature);
        var text = breeze.ToString();
        if (!string.Equals(valueLabel.Text, text, StringComparison.Ordinal))
        {
            valueLabel.Text = text;
        }
    }

    private static bool TryGetCecilyPlayer(NEnergyCounter energyCounter, out Player player)
    {
        player = null;

        if (EnergyCounterPlayerField?.GetValue(energyCounter) is not Player resolvedPlayer)
        {
            return false;
        }

        var characterId = resolvedPlayer.Character?.Id.Entry;
        if (!string.Equals(characterId, CecilyIds.Character, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        player = resolvedPlayer;
        return true;
    }
}
