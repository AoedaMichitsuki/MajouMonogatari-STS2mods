using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MajouMonogatari_STS2mods.Characters.Cecily.Powers;

namespace MajouMonogatari_STS2mods.Shared.Resources.Breeze;

/// <summary>
/// 微风资源服务层。
/// 职责：
/// - 对外提供读/判定/增减接口。
/// - 封装对 PowerCmd 的调用细节，避免业务层直接操作 Power。
/// </summary>
public static class BreezeService
{
    /// <summary>
    /// 获取当前微风值；空对象按 0 处理。
    /// </summary>
    public static int GetCurrent(Creature creature)
    {
        if (creature == null)
        {
            return 0;
        }

        return creature.GetPowerAmount<BreezePower>();
    }

    /// <summary>
    /// 是否满足消耗条件。
    /// </summary>
    public static bool CanSpend(Creature creature, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return GetCurrent(creature) >= amount;
    }

    /// <summary>
    /// 增加微风。
    /// </summary>
    public static async Task Gain(Creature creature, int amount, Creature applier, CardModel cardSource, bool silent = true)
    {
        if (creature == null || amount <= 0)
        {
            return;
        }

        var power = ModelDb.Power<BreezePower>();
        var actualApplier = applier ?? creature;

        await PowerCmd.Apply(power, creature, amount, actualApplier, cardSource, silent);
    }

    /// <summary>
    /// 消耗微风；不足时返回 false，不做变更。
    /// </summary>
    public static async Task<bool> Spend(Creature creature, int amount, Creature applier, CardModel cardSource, bool silent = true)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (creature == null || !CanSpend(creature, amount))
        {
            return false;
        }

        var breeze = creature.GetPower<BreezePower>();
        if (breeze == null)
        {
            return false;
        }

        var actualApplier = applier ?? creature;
        await PowerCmd.ModifyAmount(breeze, -amount, actualApplier, cardSource, silent);
        return true;
    }
}
