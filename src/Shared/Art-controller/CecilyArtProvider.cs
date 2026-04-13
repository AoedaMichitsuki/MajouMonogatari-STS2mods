using System.Collections.Generic;
using MajouMonogatari_STS2mods.Characters.Cecily;

namespace MajouMonogatari_STS2mods.Shared.ArtController;

/// <summary>
/// Cecily 角色美术资源“单一入口”：
/// - 业务层（卡牌/角色/Power）只依赖这个 Provider，不直连目录结构。
/// - 美术目录后续调整时，只需改这里，不会影响玩法代码。
/// </summary>
public sealed class CecilyArtProvider : ICardArtProvider, ICharacterArtProvider, IPowerArtProvider, IArtManifestProvider
{
    public static CecilyArtProvider Instance { get; } = new();

    private const string Root = "Assets/cecily";

    // 卡牌缺省占位图（项目初期可先用一张临时图跑通流程）。
    private static readonly string CardFallback = AssetPathUtil.ResPath("icon.svg");

    private CecilyArtProvider()
    {
    }

    public string CharacterVisualScenePath => AssetPathUtil.ResPath(Root, "character/scenes/cecily_visual.tscn");
    public string CharacterIconTexturePath => AssetPathUtil.ResPath(Root, "character/ui/icon_texture.png");
    public string CharacterSelectIconPath => AssetPathUtil.ResPath(Root, "character/ui/char_select_icon.png");
    public string CharacterSelectLockedIconPath => AssetPathUtil.ResPath(Root, "character/ui/char_select_locked.png");
    public string CharacterMapMarkerPath => AssetPathUtil.ResPath(Root, "character/ui/map_marker.png");

    public string CardBigEnergyIconPath => AssetPathUtil.ResPath(Root, "character/ui/big_energy.png");
    public string CardTextEnergyIconPath => AssetPathUtil.ResPath(Root, "character/ui/text_energy.png");

    public string GetCardPortraitPath(string cardEntryId)
    {
        var file = AssetPathUtil.NormalizeEntryId(cardEntryId) + ".png";
        var primary = AssetPathUtil.ResPath(Root, "cards/portraits", file);
        return AssetPathUtil.ResolveOrFallback(primary, CardFallback);
    }

    public string GetCardMiniPortraitPath(string cardEntryId)
    {
        // 当前阶段先与常规立绘复用同一套路径，后续可切换到单独 mini 目录。
        return GetCardPortraitPath(cardEntryId);
    }

    public string GetCardBetaPortraitPath(string cardEntryId)
    {
        var file = AssetPathUtil.NormalizeEntryId(cardEntryId) + ".png";
        var primary = AssetPathUtil.ResPath(Root, "cards/beta", file);
        return AssetPathUtil.ResolveOrFallback(primary, GetCardPortraitPath(cardEntryId));
    }

    public string GetPowerSmallIconPath(string powerEntryId)
    {
        var file = AssetPathUtil.NormalizeEntryId(powerEntryId) + ".png";
        var primary = AssetPathUtil.ResPath(Root, "powers/small", file);
        return AssetPathUtil.ResolveOrFallback(primary, CardFallback);
    }

    public string GetPowerBigIconPath(string powerEntryId)
    {
        var file = AssetPathUtil.NormalizeEntryId(powerEntryId) + ".png";
        var primary = AssetPathUtil.ResPath(Root, "powers/big", file);
        return AssetPathUtil.ResolveOrFallback(primary, GetPowerSmallIconPath(powerEntryId));
    }

    public string GetPowerBigBetaIconPath(string powerEntryId)
    {
        // 第一版先复用 big 图；后续如果有 beta 图，可拆到独立目录。
        return GetPowerBigIconPath(powerEntryId);
    }

    public IReadOnlyList<string> GetRequiredAssetPaths()
    {
        var result = new List<string>
        {
            CharacterVisualScenePath,
            CharacterIconTexturePath,
            CharacterSelectIconPath,
            CharacterSelectLockedIconPath,
            CharacterMapMarkerPath,
            CardBigEnergyIconPath,
            CardTextEnergyIconPath,
            GetPowerSmallIconPath(CecilyIds.BreezePower),
            GetPowerBigIconPath(CecilyIds.BreezePower),
            GetCardPortraitPath(CecilyIds.StrikeCard),
            GetCardPortraitPath(CecilyIds.DefendCard),
            GetCardPortraitPath(CecilyIds.WindBulletCard),
            GetCardPortraitPath(CecilyIds.SpringTuftCard)
        };

        return result;
    }
}
