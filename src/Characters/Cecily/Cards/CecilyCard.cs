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
    public override string CustomPortraitPath => CecilyArtProvider.Instance.GetCardPortraitPath(Id.Entry);
    public override string PortraitPath => CecilyArtProvider.Instance.GetCardMiniPortraitPath(Id.Entry);
    public override string BetaPortraitPath => CecilyArtProvider.Instance.GetCardBetaPortraitPath(Id.Entry);
}
