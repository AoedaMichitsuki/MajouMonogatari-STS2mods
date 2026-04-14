using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MajouMonogatari_STS2mods.Shared.Art;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards;

[Pool(typeof(CecilyCardPool))]
public abstract class CecilyCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    public override string CustomPortraitPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetCardPortraitPath(Id.Entry), base.CustomPortraitPath);

    public override string PortraitPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetCardMiniPortraitPath(Id.Entry), base.PortraitPath);

    public override string BetaPortraitPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.GetCardBetaPortraitPath(Id.Entry), base.BetaPortraitPath);
}
