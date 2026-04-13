using System;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;
using MajouMonogatari_STS2mods.Characters.Cecily.Powers;
using MajouMonogatari_STS2mods.Characters.Cecily.Relics;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Core;

public static class ModelBootstrap
{
    private static readonly Type[] ModelTypes =
    [
        typeof(CecilyCharacter),
        typeof(CecilyCardPool),
        typeof(CecilyRelicPool),
        typeof(CecilyPotionPool),
        typeof(CecilyStrikeCard),
        typeof(CecilyDefendCard),
        typeof(CecilyWindBulletCard),
        typeof(CecilySpringTuftCard),
        typeof(BreezePower),
        typeof(BornMagicWindRelic)
    ];

    public static void RegisterAll()
    {
        foreach (var type in ModelTypes)
        {
            if (ModelDb.Contains(type))
            {
                continue;
            }

            ModelDb.Inject(type);
            ModLog.Info($"Injected model: {type.FullName}");
        }
    }
}
