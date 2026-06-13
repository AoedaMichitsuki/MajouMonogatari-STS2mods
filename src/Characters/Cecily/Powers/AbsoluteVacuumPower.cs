using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MajouMonogatari_STS2mods.Shared.Keywords.Locked;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Powers;

[CustomID(CecilyIds.AbsoluteVacuumPower)]
public class AbsoluteVacuumPower : CecilyPower
{
    private int _pendingEnergy;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var player = Owner?.Player;
        if (side == CombatSide.Player && player != null && LockedRuntimeState.CountLockedInHand(player) == 0)
        {
            _pendingEnergy += Amount;
            Flash();
        }

        return Task.CompletedTask;
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != Owner?.Player || _pendingEnergy <= 0)
        {
            return;
        }

        var energyToGain = _pendingEnergy;
        _pendingEnergy = 0;
        Flash();
        await PlayerCmd.GainEnergy(energyToGain, player);
    }
}
