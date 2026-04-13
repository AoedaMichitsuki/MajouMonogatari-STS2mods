using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Shared.ArtController;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards;

/// <summary>
/// 希赛莉卡牌基类。
/// 作用：
/// 1. 统一绑定卡池（PoolAttribute）。
/// 2. 统一接入美术资源接口，避免每张卡重复写路径逻辑。
/// 3. 给后续“共通词条/共通表现”预留集中扩展点。
/// </summary>
[Pool(typeof(CecilyCardPool))]
public abstract class CecilyCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    /// <summary>
    /// 常规大图立绘路径。
    /// </summary>
    public override string CustomPortraitPath => CecilyArtProvider.Instance.GetCardPortraitPath(Id.Entry);

    /// <summary>
    /// 小图立绘路径（当前先复用常规立绘）。
    /// </summary>
    public override string PortraitPath => CecilyArtProvider.Instance.GetCardMiniPortraitPath(Id.Entry);

    /// <summary>
    /// Beta 立绘路径。
    /// </summary>
    public override string BetaPortraitPath => CecilyArtProvider.Instance.GetCardBetaPortraitPath(Id.Entry);
}
