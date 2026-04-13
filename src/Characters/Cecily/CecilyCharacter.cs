using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;
using MajouMonogatari_STS2mods.Shared.ArtController;

namespace MajouMonogatari_STS2mods.Characters.Cecily;

/// <summary>
/// 希赛莉角色模型（当前为最小可运行版本）。
/// 设计目标：
/// 1. 跑通角色进入战斗与基础牌组。
/// 2. 资源路径全部走美术接口层，便于后续替换资源。
/// </summary>
[CustomID(CecilyIds.Character)]
public sealed class CecilyCharacter : PlaceholderCharacterModel
{
    /// <summary>
    /// 角色主题色，用于名字/UI 等视觉元素。
    /// </summary>
    public static readonly Color CecilyColor = new("4AA6D9");

    /// <summary>
    /// 当前阶段覆盖 Silent 资源，保证在美术未齐全时可运行。
    /// </summary>
    public override string PlaceholderID => CecilyIds.PlaceholderCharacter;

    public override Color NameColor => CecilyColor;
    public override CharacterGender Gender => CharacterGender.Feminine;

    /// <summary>
    /// 文档给定的初始数值。
    /// </summary>
    public override int StartingHp => 70;
    public override int StartingGold => 99;

    /// <summary>
    /// 初始牌组（basic）：
    /// - strike x4
    /// - defend x4
    /// - wind_bullet x1
    /// - spring_tuft x1
    /// </summary>
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<CecilyStrikeCard>(),
        ModelDb.Card<CecilyStrikeCard>(),
        ModelDb.Card<CecilyStrikeCard>(),
        ModelDb.Card<CecilyStrikeCard>(),
        ModelDb.Card<CecilyDefendCard>(),
        ModelDb.Card<CecilyDefendCard>(),
        ModelDb.Card<CecilyDefendCard>(),
        ModelDb.Card<CecilyDefendCard>(),
        ModelDb.Card<CecilyWindBulletCard>(),
        ModelDb.Card<CecilySpringTuftCard>()
    ];

    /// <summary>
    /// 初始遗物后续再补（当前保留空集合）。
    /// </summary>
    public override IReadOnlyList<RelicModel> StartingRelics => [];

    /// <summary>
    /// 三大池配置。
    /// </summary>
    public override CardPoolModel CardPool => ModelDb.CardPool<CecilyCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SharedRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

    /// <summary>
    /// 角色美术资源路径（均由接口层提供，避免硬编码散落）。
    /// </summary>
    public override string CustomVisualPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CharacterVisualScenePath, base.CustomVisualPath);

    public override string CustomIconTexturePath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CharacterIconTexturePath, base.CustomIconTexturePath);

    public override string CustomCharacterSelectIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CharacterSelectIconPath, base.CustomCharacterSelectIconPath);

    public override string CustomCharacterSelectLockedIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CharacterSelectLockedIconPath, base.CustomCharacterSelectLockedIconPath);

    public override string CustomMapMarkerPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CharacterMapMarkerPath, base.CustomMapMarkerPath);

    protected override CharacterModel UnlocksAfterRunAs => null;
}
