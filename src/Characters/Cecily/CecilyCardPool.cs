using BaseLib.Abstracts;
using Godot;
using MajouMonogatari_STS2mods.Shared.ArtController;

namespace MajouMonogatari_STS2mods.Characters.Cecily;

/// <summary>
/// 希赛莉卡池定义。
/// 这里主要提供：
/// - 卡背调色（HSV）
/// - 牌库入口颜色
/// - 能量图标路径
/// </summary>
public sealed class CecilyCardPool : CustomCardPoolModel
{
    /// <summary>
    /// 非显示名，而是卡池逻辑标识。
    /// </summary>
    public override string Title => CecilyIds.Character;

    /// <summary>
    /// 卡背基础色调（通过 shader 应用于默认卡框）。
    /// </summary>
    public override float H => 0.53f;
    public override float S => 0.78f;
    public override float V => 0.92f;

    /// <summary>
    /// 牌库界面中的小卡颜色与能量描边颜色。
    /// </summary>
    public override Color DeckEntryCardColor => new("2D7298");
    public override Color EnergyOutlineColor => new("1E4A63");

    /// <summary>
    /// 卡池是否属于无色池。
    /// </summary>
    public override bool IsColorless => false;

    /// <summary>
    /// 卡牌能量图标资源。
    /// </summary>
    public override string BigEnergyIconPath => CecilyArtProvider.Instance.CardBigEnergyIconPath;
    public override string TextEnergyIconPath => CecilyArtProvider.Instance.CardTextEnergyIconPath;
}
