using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;

/// <summary>
/// 防御（basic）
/// - 费用：1
/// - 类型：技能
/// - 数值：格挡 5（升级 +3）
/// </summary>
[CustomID(CecilyIds.DefendCard)]
public sealed class CecilyDefendCard() : CecilyCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    /// <summary>
    /// 标记为 Defend，便于标签联动。
    /// </summary>
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    /// <summary>
    /// 动态变量：格挡值。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    /// <summary>
    /// 出牌效果：获得格挡。
    /// </summary>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
    }

    /// <summary>
    /// 升级：格挡 +3（5 -> 8）。
    /// </summary>
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
