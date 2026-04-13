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
/// 打击（basic）
/// - 费用：1
/// - 类型：攻击
/// - 数值：6（升级 +3）
/// </summary>
[CustomID(CecilyIds.StrikeCard)]
public sealed class CecilyStrikeCard() : CecilyCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    /// <summary>
    /// 标记为 Strike，便于遗物/能力按标签联动。
    /// </summary>
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    /// <summary>
    /// 动态变量：伤害值。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    /// <summary>
    /// 出牌效果：对目标执行一次标准攻击。
    /// </summary>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await CommonActions.CardAttack(this, cardPlay.Target).Execute(choiceContext);
    }

    /// <summary>
    /// 升级：伤害 +3（6 -> 9）。
    /// </summary>
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
