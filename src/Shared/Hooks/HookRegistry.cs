using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MajouMonogatari_STS2mods.Shared.Core;
using MajouMonogatari_STS2mods.Shared.Rules;

namespace MajouMonogatari_STS2mods.Shared.Hooks;

/// <summary>
/// Hook 统一注册中心。
/// 目标：
/// - 所有 Hook 相关补丁集中在这里，避免散落。
/// - 通过 Harmony 给 Hook 静态方法打补丁，在不侵入游戏主逻辑的情况下挂接规则。
/// </summary>
public static class HookRegistry
{
    /// <summary>
    /// 当前阶段需要覆盖的核心 Hook 名称集合。
    /// </summary>
    private static readonly string[] CoreHookNames =
    {
        "AfterCardPlayed",
        "BeforeCardPlayed",
        "ShouldPlay",
        "BeforeHandDraw",
        "ShouldDraw",
        "AfterCardDrawn",
        "AfterCardDiscarded",
        "AfterCardExhausted",
        "AfterCardChangedPiles",
        "BeforeTurnEnd",
        "AfterTurnEnd",
        "BeforeSideTurnStart",
        "AfterSideTurnStart",
        "AfterEnergyReset",
        "ShouldPlayerResetEnergy",
        "AfterBlockGained",
        "ModifyCardPlayResultPileTypeAndPosition"
    };

    private static readonly object Gate = new();
    private static readonly HashSet<string> SeenHookCalls = new(StringComparer.Ordinal);

    private static bool _registered;
    private static Harmony _harmony;
    private static string _harmonyId = string.Empty;

    /// <summary>
    /// 注册所有 Hook 补丁（幂等）。
    /// </summary>
    public static bool RegisterAll(string modId)
    {
        lock (Gate)
        {
            if (_registered)
            {
                return true;
            }

            var hookType = Type.GetType("MegaCrit.Sts2.Core.Hooks.Hook, sts2");
            if (hookType is null)
            {
                ModLog.Warn("Could not locate MegaCrit.Sts2.Core.Hooks.Hook in sts2.dll.");
                return false;
            }

            var corePostfix = AccessTools.Method(typeof(HookRegistry), nameof(CoreHookPostfix));
            var shouldPlayPostfix = AccessTools.Method(typeof(HookRegistry), nameof(ShouldPlayRulePostfix));
            if (corePostfix is null || shouldPlayPostfix is null)
            {
                ModLog.Error("Could not resolve HookRegistry patch methods.");
                return false;
            }

            _harmonyId = $"{modId}.hook-registry";
            _harmony = new Harmony(_harmonyId);
            var patchedCount = 0;

            foreach (var hookName in CoreHookNames)
            {
                var matchingMethods = hookType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => string.Equals(m.Name, hookName, StringComparison.Ordinal))
                    .ToArray();

                if (matchingMethods.Length == 0)
                {
                    ModLog.Warn($"Hook.{hookName} was not found.");
                    continue;
                }

                foreach (var method in matchingMethods)
                {
                    try
                    {
                        // 基础后置：用于追踪 Hook 实际触发情况（调试/验收很有用）。
                        _harmony.Patch(method, postfix: new HarmonyMethod(corePostfix));
                        patchedCount++;

                        // 对 ShouldPlay 额外挂接“微风不足不可打出”规则。
                        if (string.Equals(method.Name, "ShouldPlay", StringComparison.Ordinal))
                        {
                            _harmony.Patch(method, postfix: new HarmonyMethod(shouldPlayPostfix));
                            patchedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        ModLog.Warn($"Failed to patch Hook.{method.Name}: {ex.Message}");
                    }
                }
            }

            if (patchedCount == 0)
            {
                ModLog.Warn("HookRegistry did not patch any hooks.");
                return false;
            }

            _registered = true;
            ModLog.Info($"HookRegistry patched {patchedCount} hook method(s).");
            return true;
        }
    }

    /// <summary>
    /// 反注册当前注册器打过的补丁。
    /// </summary>
    public static void UnregisterAll()
    {
        lock (Gate)
        {
            if (!_registered || _harmony == null || string.IsNullOrWhiteSpace(_harmonyId))
            {
                return;
            }

            _harmony.UnpatchAll(_harmonyId);
            _registered = false;
            SeenHookCalls.Clear();
            ModLog.Info("HookRegistry unpatched all hook interceptors.");
        }
    }

    /// <summary>
    /// 通用 Hook 观察后置：每种 Hook 名称只打印一次，避免日志刷屏。
    /// </summary>
    private static void CoreHookPostfix(MethodBase __originalMethod)
    {
        if (__originalMethod is null)
        {
            return;
        }

        lock (Gate)
        {
            if (!SeenHookCalls.Add(__originalMethod.Name))
            {
                return;
            }
        }

        ModLog.Info($"Observed hook call: {__originalMethod.Name}");
    }

    /// <summary>
    /// ShouldPlay 规则后置。
    /// 若 BreezePlayRule 判定应拦截，则强制把结果改为 false。
    /// </summary>
    private static void ShouldPlayRulePostfix(
        CombatState combatState,
        CardModel card,
        ref AbstractModel preventer,
        AutoPlayType autoPlayType,
        ref bool __result)
    {
        if (!__result || card == null)
        {
            return;
        }

        if (!BreezePlayRule.ShouldBlockPlay(card, ref preventer))
        {
            return;
        }

        __result = false;
    }
}
