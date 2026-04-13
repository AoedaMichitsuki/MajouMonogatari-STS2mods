using System.Threading.Tasks;
using MajouMonogatari_STS2mods.Characters.Cecily.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MajouMonogatari_STS2mods.Shared.Resources.Breeze;

public static class BreezeService
{
    public static int GetCurrent(Creature creature)
    {
        if (creature == null)
        {
            return 0;
        }

        return creature.GetPowerAmount<BreezePower>();
    }

    public static bool CanSpend(Creature creature, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return GetCurrent(creature) >= amount;
    }

    public static Task Gain(Creature creature, int amount, Creature applier, CardModel sourceCard, bool silent = true)
    {
        if (creature == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        return PowerCmd.Apply(ModelDb.Power<BreezePower>(), creature, amount, applier ?? creature, sourceCard, silent);
    }

    public static async Task<bool> Spend(Creature creature, int amount, Creature applier, CardModel sourceCard, bool silent = true)
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

        await PowerCmd.ModifyAmount(breeze, -amount, applier ?? creature, sourceCard, silent);
        return true;
    }
}
