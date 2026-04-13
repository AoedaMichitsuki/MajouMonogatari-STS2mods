using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using System.Collections.Generic;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;
using MajouMonogatari_STS2mods.Characters.Cecily.Relics;
using MajouMonogatari_STS2mods.Shared.Art;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MajouMonogatari_STS2mods.Characters.Cecily;

[CustomID(CecilyIds.Character)]
public class CecilyCharacter : PlaceholderCharacterModel
{
    public static readonly Color CecilyColor = new("4aa6d9");

    public override string PlaceholderID => CecilyIds.PlaceholderCharacter;

    public override Color NameColor => CecilyColor;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;
    public override int StartingGold => 99;

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

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BornMagicWindRelic>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<CecilyCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<CecilyRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<CecilyPotionPool>();

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
