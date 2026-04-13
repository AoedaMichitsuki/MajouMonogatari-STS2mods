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
/// 春絮（basic）
/// - 费用：0
/// - 类型：技能
/// - 效果：获得 2 点格挡，重复 2 次（升级后 3 次）。
///
/// 说明：
/// - 流场（左/右）分支后续由 FlowEvaluator 接管。
/// </summary>
[CustomID(CecilyIds.SpringTuftCard)]
public sealed class CecilySpringTuftCard() : CecilyCard(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(2, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var repeatCount = DynamicVars.Repeat.IntValue;
        if (repeatCount < 1)
        {
            repeatCount = 1;
        }

        // 逐段加格挡，保证与触发型效果（如每次获得格挡）兼容。
        for (var i = 0; i < repeatCount; i++)
        {
            await CommonActions.CardBlock(this, cardPlay);
        }

        // Flow(Left/Right) behavior is intentionally deferred to the dedicated Flow rule/evaluator implementation.
    }

    protected override void OnUpgrade()
    {
        // 升级增加重复次数：2 -> 3。
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
