namespace MajouMonogatari_STS2mods.Characters.Cecily;

/// <summary>
/// 希赛莉相关 ID 常量。
/// 说明：
/// - 统一集中管理，避免字符串分散硬编码。
/// - EntryId 也是资源命名与本地化 key 的重要锚点。
/// </summary>
public static class CecilyIds
{
    /// <summary>角色主 ID。</summary>
    public const string Character = "majoumonogatari-sts2mods.cecily";

    /// <summary>卡池与遗物池 ID。</summary>
    public const string CardPool = Character + ".cards";
    public const string RelicPool = Character + ".relics";

    /// <summary>共享资源：微风 Power。</summary>
    public const string BreezePower = Character + ".breeze_power";

    /// <summary>basic 卡牌 ID。</summary>
    public const string StrikeCard = Character + ".strike";
    public const string DefendCard = Character + ".defend";
    public const string WindBulletCard = Character + ".wind_bullet";
    public const string SpringTuftCard = Character + ".spring_tuft";
}
