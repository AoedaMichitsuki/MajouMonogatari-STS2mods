using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MajouMonogatari_STS2mods.Shared.Keywords.Chant;

public interface IChantCard
{
    Task ResolveChant(PlayerChoiceContext choiceContext, ChantResolutionContext chantContext);
}
