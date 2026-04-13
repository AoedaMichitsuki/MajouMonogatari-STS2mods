using System;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MegaCrit.Sts2.Core.Models;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;
using MajouMonogatari_STS2mods.Characters.Cecily.Powers;

namespace MajouMonogatari_STS2mods.Shared.Core;

/// <summary>
/// 模型注入引导器。
/// 统一维护“本 Mod 需要注入到 ModelDb 的类型列表”。
/// </summary>
public static class ModelBootstrap
{
    private static readonly Type[] ModelTypes =
    [
        typeof(CecilyCharacter),
        typeof(CecilyCardPool),
        typeof(CecilyStrikeCard),
        typeof(CecilyDefendCard),
        typeof(CecilyWindBulletCard),
        typeof(CecilySpringTuftCard),
        typeof(BreezePower)
    ];

    /// <summary>
    /// 将所有模型注入到 ModelDb。
    /// 这里做幂等判断（Contains），防止重复注入。
    /// </summary>
    public static void RegisterAll()
    {
        foreach (var modelType in ModelTypes)
        {
            if (ModelDb.Contains(modelType))
            {
                continue;
            }

            ModelDb.Inject(modelType);
            ModLog.Info($"Injected model: {modelType.FullName}");
        }
    }
}
