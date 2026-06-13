namespace MajouMonogatari_STS2mods.Shared.Keywords.Flow;

public interface IFlowCard
{
    bool HasFlowLeft { get; }
    bool HasFlowRight { get; }
    bool HasFlowVain { get; }
}
