namespace MajouMonogatari_STS2mods.Shared.Resources.Breeze;

/// <summary>
/// 声明“该卡牌需要消耗微风”。
/// 规则层（ShouldPlay）只识别这个接口，不依赖具体卡牌类型。
/// </summary>
public interface IBreezeCostCard
{
    /// <summary>
    /// 打出该卡所需的微风点数。
    /// </summary>
    int BreezeCost { get; }
}
