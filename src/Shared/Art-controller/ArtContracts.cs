using System.Collections.Generic;

namespace MajouMonogatari_STS2mods.Shared.ArtController;

/// <summary>
/// 约定卡牌立绘资源访问能力。
/// - 这里返回的是 Godot 资源路径（res://...）。
/// - 调用方只关心“拿到哪张卡对应的路径”，不关心具体目录布局。
/// </summary>
public interface ICardArtProvider
{
    /// <summary>
    /// 获取卡牌常规立绘（全图）路径。
    /// </summary>
    string GetCardPortraitPath(string cardEntryId);

    /// <summary>
    /// 获取卡牌小图路径（手牌/界面常用）。
    /// </summary>
    string GetCardMiniPortraitPath(string cardEntryId);

    /// <summary>
    /// 获取卡牌 Beta 立绘路径。
    /// </summary>
    string GetCardBetaPortraitPath(string cardEntryId);
}

/// <summary>
/// 约定角色 UI 与场景资源访问能力。
/// </summary>
public interface ICharacterArtProvider
{
    string CharacterVisualScenePath { get; }
    string CharacterIconTexturePath { get; }
    string CharacterSelectIconPath { get; }
    string CharacterSelectLockedIconPath { get; }
    string CharacterMapMarkerPath { get; }
    string CardBigEnergyIconPath { get; }
    string CardTextEnergyIconPath { get; }
}

/// <summary>
/// 约定 Power 图标资源访问能力。
/// </summary>
public interface IPowerArtProvider
{
    string GetPowerSmallIconPath(string powerEntryId);
    string GetPowerBigIconPath(string powerEntryId);
    string GetPowerBigBetaIconPath(string powerEntryId);
}

/// <summary>
/// 约定“美术交付清单”能力。
/// 便于把当前角色需要准备的资源路径集中导出给美术。
/// </summary>
public interface IArtManifestProvider
{
    IReadOnlyList<string> GetRequiredAssetPaths();
}
