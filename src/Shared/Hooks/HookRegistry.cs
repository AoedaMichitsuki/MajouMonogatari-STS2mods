using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Characters.Cecily.Relics;
using MajouMonogatari_STS2mods.Shared.Animation;
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

/// <summary>
/// 统一维护本 Mod 的 Harmony 注入点。
/// 
/// 这个类负责三件事：
/// 1) 玩法规则注入：Breeze 可打性校验、战斗开始重置、挡格触发微风等。
/// 2) Flow 快照时序注入：确保 OnPlay 读取到的是“离手前”真实手牌站位。
/// 3) Breeze UI 注入：把自定义计数器挂到 NEnergyCounter，并在每帧更新数值/旋转层。
/// 
/// 设计目标是“集中注册 + 幂等执行”：所有 Patch 都在 RegisterAll 内一次性完成，重复初始化不重复打补丁。
/// </summary>
public static class HookRegistry
{
    // Breeze 计数器实例的根节点名（挂在 NEnergyCounter 下）。
    private const string BreezeCounterNodeName = "CecilyBreezeCounter";
    // Breeze 计数器场景路径（来自 mod 资源）。
    private const string BreezeCounterScenePath = "res://Assets/cecily/powers/star_counter.tscn";
    // Breeze 数值 Label 路径（相对于 NEnergyCounter）。
    private const string BreezeCounterValuePath = "CecilyBreezeCounter/MarginContainer/CountLabel";
    // 旋转装饰层路径（用于转动特效）。
    private const string BreezeCounterLayer1Path = "CecilyBreezeCounter/Icon/RotationLayers/Layer1";
    private const string BreezeCounterLayer2Path = "CecilyBreezeCounter/Icon/RotationLayers/Layer2";

    // 通过反射读取 NEnergyCounter 内部 _player 字段，用于判定“该能量计是否属于 Cecily”。
    private static readonly FieldInfo EnergyCounterPlayerField = AccessTools.Field(
        "MegaCrit.Sts2.Core.Nodes.Combat.NEnergyCounter:_player");

    // 初始化幂等锁：防止 ModInitializer/_Ready 重入时重复 Patch。
    private static readonly object Gate = new();
    private static bool _registered;

    /// <summary>
    /// 统一注册所有补丁。
    /// </summary>
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
            // 出牌合法性：用于 Breeze 消耗牌的“微风不足不可打”。
            PatchByName(harmony, hookType, "ShouldPlay", postfixName: nameof(ShouldPlayPostfix));
            // 出牌前后：绑定/清理动画上下文与 Flow 快照。
            PatchByName(harmony, hookType, "BeforeCardPlayed", prefixName: nameof(BeforeCardPlayedPrefix));
            PatchByName(harmony, hookType, "AfterCardPlayed", nameof(AfterCardPlayedPostfix));
            // 角色资源相关：挡格触发遗物加微风；战斗开始重置状态。
            PatchByName(harmony, hookType, "AfterBlockGained", nameof(AfterBlockGainedPostfix));
            PatchByName(harmony, hookType, "BeforeCombatStart", nameof(BeforeCombatStartPostfix));
            // UI：在能量计节点生命周期中挂载并刷新 Breeze 计数器。
            PatchEnergyCounterByName(harmony, "_Ready", nameof(EnergyCounterReadyPostfix));
            PatchEnergyCounterByName(harmony, "_Process", nameof(EnergyCounterProcessPostfix));
            // 动画桥接：读取引擎触发，转发到自定义动画路由。
            PatchCreatureByName(harmony, nameof(NCreature.SetAnimationTrigger), nameof(CreatureSetAnimationTriggerPostfix));
            // Flow 关键时序：手牌移除前捕获“离手瞬间”的站位快照。
            PatchCardPileByName(harmony, nameof(CardPile.RemoveInternal), nameof(CardPileRemoveInternalPrefix));

            _registered = true;
            ModLog.Info("Hook registry patched.");
        }
    }

    /// <summary>
    /// 通用 Hook.* 方法补丁器。
    /// 支持 prefix/postfix 任意组合；同名重载会全部 patch。
    /// </summary>
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

    /// <summary>
    /// 给 NEnergyCounter 指定方法打 postfix（用于 UI 挂载/刷新）。
    /// </summary>
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

    /// <summary>
    /// 给 CardPile 指定方法打 prefix（用于 Flow 离手前快照）。
    /// </summary>
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

    /// <summary>
    /// 给 NCreature 指定方法打 postfix（用于动画触发桥接）。
    /// </summary>
    private static void PatchCreatureByName(Harmony harmony, string targetMethodName, string postfixName)
    {
        var postfix = AccessTools.Method(typeof(HookRegistry), postfixName);
        if (postfix == null)
        {
            ModLog.Error($"Missing NCreature postfix method: {postfixName}");
            return;
        }

        var targetMethod = AccessTools.Method(typeof(NCreature), targetMethodName);
        if (targetMethod == null)
        {
            ModLog.Warn($"Missing NCreature method: {targetMethodName}");
            return;
        }

        harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));
    }

    /// <summary>
    /// Hook.ShouldPlay 后置：
    /// 1) 刷新当前手牌的 Flow 快照；
    /// 2) 对实现了 IBreezeCostCard 的牌执行 Breeze 资源可打性校验。
    /// </summary>
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

    /// <summary>
    /// Hook.BeforeCardPlayed 前置：
    /// 记录当前 CardPlay 的动画上下文，并把 Flow 快照绑定到这次 CardPlay。
    /// </summary>
    private static void BeforeCardPlayedPrefix(CombatState combatState, CardPlay cardPlay)
    {
        if (cardPlay?.Card == null)
        {
            return;
        }

        CreatureAnimationRuntime.BeginCardPlay(cardPlay);

        // Snapshot by CardPlay so OnPlay always reads the exact play instance.
        FlowRuntimeState.CaptureFromHand(cardPlay);
    }

    /// <summary>
    /// Hook.AfterCardPlayed 后置：
    /// 结束动画上下文，清理本次 CardPlay 快照，回收并刷新手牌级快照。
    /// </summary>
    private static void AfterCardPlayedPostfix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CreatureAnimationRuntime.EndCardPlay(cardPlay);
        FlowRuntimeState.Clear(cardPlay);
        FlowRuntimeState.RefreshFromHand(combatState);
    }

    /// <summary>
    /// CardPile.RemoveInternal 前置：
    /// 仅在 Hand 牌堆移除时捕获快照，确保拿到“离开手牌前”的真实索引。
    /// </summary>
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

    /// <summary>
    /// 把引擎的 SetAnimationTrigger 转发给自定义动画运行时。
    /// </summary>
    private static void CreatureSetAnimationTriggerPostfix(NCreature __instance, string trigger)
    {
        CreatureAnimationRuntime.TryHandleEngineTrigger(__instance, trigger);
    }

    /// <summary>
    /// 开战清理：
    /// - 清空 Flow 状态；
    /// - 重置所有玩家的 Breeze 运行时值。
    /// </summary>
    private static void BeforeCombatStartPostfix(object runState, CombatState combatState)
    {
        FlowRuntimeState.ClearAll();
        BreezeService.ResetForCombat(combatState);
    }

    /// <summary>
    /// 挡格后处理：
    /// 当玩家拥有 BornMagicWindRelic 且本次获得挡格 > 0 时，增加 1 点 Breeze。
    /// </summary>
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

    /// <summary>
    /// NEnergyCounter._Ready 后置：
    /// 只给 Cecily 玩家挂 Breeze 计数器场景，并初始化显示值。
    /// </summary>
    private static void EnergyCounterReadyPostfix(NEnergyCounter __instance)
    {
        if (!TryGetCecilyPlayer(__instance, out var player))
        {
            return;
        }

        if (__instance.GetNodeOrNull<Control>(BreezeCounterNodeName) != null)
        {
            return;
        }

        var scene = ResourceLoader.Load<PackedScene>(BreezeCounterScenePath);
        if (scene == null)
        {
            ModLog.Warn($"Failed to load Breeze counter scene: {BreezeCounterScenePath}");
            return;
        }

        if (scene.Instantiate() is not Control counter)
        {
            ModLog.Warn($"Breeze counter scene root is not Control: {BreezeCounterScenePath}");
            return;
        }

        counter.Name = BreezeCounterNodeName;
        counter.MouseFilter = Control.MouseFilterEnum.Ignore;
        counter.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        counter.Position = new Vector2(0f, -128f);
        counter.Size = new Vector2(128f, 128f);
        counter.Scale = new Vector2(1f, 1f);
        __instance.AddChild(counter);

        // 运行时再设一次 Pivot：以实际 Size 为准，避免场景尺寸调整后旋转轴偏移。
        if (__instance.GetNodeOrNull<Control>(BreezeCounterLayer1Path) is { } layer1)
        {
            layer1.PivotOffset = layer1.Size * 0.5f;
        }

        if (__instance.GetNodeOrNull<Control>(BreezeCounterLayer2Path) is { } layer2)
        {
            layer2.PivotOffset = layer2.Size * 0.5f;
        }

        var valueLabel = __instance.GetNodeOrNull<Label>(BreezeCounterValuePath);
        if (valueLabel != null)
        {
            valueLabel.Text = BreezeService.GetCurrent(player.Creature).ToString();
        }
    }

    /// <summary>
    /// NEnergyCounter._Process 后置：
    /// - 每帧同步 Breeze 文本；
    /// - 推动两层装饰旋转，形成动态计数器效果。
    /// </summary>
    private static void EnergyCounterProcessPostfix(NEnergyCounter __instance)
    {
        if (!TryGetCecilyPlayer(__instance, out var player))
        {
            return;
        }

        var counter = __instance.GetNodeOrNull<Control>(BreezeCounterNodeName);
        if (counter == null)
        {
            return;
        }

        var valueLabel = __instance.GetNodeOrNull<Label>(BreezeCounterValuePath);
        if (valueLabel != null)
        {
            var text = BreezeService.GetCurrent(player.Creature).ToString();
            if (!string.Equals(valueLabel.Text, text, StringComparison.Ordinal))
            {
                valueLabel.Text = text;
            }
        }

        var delta = (float)__instance.GetProcessDeltaTime();
        if (__instance.GetNodeOrNull<Control>(BreezeCounterLayer1Path) is { } layer1)
        {
            layer1.Rotation += 0.95f * delta;
        }

        if (__instance.GetNodeOrNull<Control>(BreezeCounterLayer2Path) is { } layer2)
        {
            layer2.Rotation -= 0.7f * delta;
        }
    }

    /// <summary>
    /// 从 NEnergyCounter 反射拿到绑定玩家，并过滤为 Cecily。
    /// </summary>
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
