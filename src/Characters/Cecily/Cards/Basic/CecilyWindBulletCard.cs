using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Characters.Cecily.Cards;
using MajouMonogatari_STS2mods.Shared.Resources.Breeze;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Cards.Basic;

/// <summary>
/// 风弹（basic）
/// - 费用：0
/// - 类型：攻击
/// - 效果：造成（6 + 当前微风）伤害，然后消耗 2 微风。
///
/// 说明：
/// - “微风不足不可打出”由 ShouldPlay 规则层统一处理。
/// - 流场（左）增益留待 Flow 机制模块接入。
/// </summary>
[CustomID(CecilyIds.WindBulletCard)]
public sealed class CecilyWindBulletCard() : CecilyCard(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy), IBreezeCostCard
{
    private const int BaseDamage = 6;

    /// <summary>
    /// 声明该卡的微风消耗，供规则层拦截读取。
    /// </summary>
    public int BreezeCost => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(BaseDamage, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner?.Creature;
        if (owner == null || cardPlay.Target == null)
        {
            return;
        }

        // 先记录消耗前微风，用于计算伤害。
        var breezeBeforeSpend = BreezeService.GetCurrent(owner);

        // 双保险：即便规则层异常漏拦，也不会在资源不足时继续执行。
        var spendSucceeded = await BreezeService.Spend(owner, BreezeCost, owner, this);
        if (!spendSucceeded)
        {
            return;
        }

        var totalDamage = BaseDamage + breezeBeforeSpend;
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, totalDamage, ValueProp.Move, owner, this);

        // Flow(Left) bonus is intentionally deferred to the dedicated Flow rule/evaluator implementation.
    }

    protected override void OnUpgrade()
    {
        // 当前版本按文档先保留“升级影响流场分支”，基础伤害不变。
    }
}
