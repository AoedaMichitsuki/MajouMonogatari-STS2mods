namespace MajouMonogatari_STS2mods.Characters.Cecily;

public static class CecilyIds
{
    // Model IDs participate in save serialization (e.g. CHARACTER.<Entry>, CARD.<Entry>).
    // Use uppercase + underscores to stay within the game's expected ModelId form.
    public const string Character = "MAJOUMONOGATARI_STS2MODS_CECILY";
    private const string Prefix = Character;
    public const string PlaceholderCharacter = "silent";

    public const string CardPool = Prefix + "_CARD_POOL";
    public const string RelicPool = Prefix + "_RELIC_POOL";
    public const string PotionPool = Prefix + "_POTION_POOL";

    public const string StrikeCard = Prefix + "_STRIKE";
    public const string DefendCard = Prefix + "_DEFEND";
    public const string WindBulletCard = Prefix + "_WIND_BULLET";
    public const string SpringTuftCard = Prefix + "_SPRING_TUFT";
    public const string CondensationCard = Prefix + "_CONDENSATION";
    public const string TrapCard = Prefix + "_TRAP";
    public const string BlossomWayCard = Prefix + "_BLOSSOM_WAY";

    public const string BreezePower = Prefix + "_BREEZE_POWER";
    public const string BornMagicWindRelic = Prefix + "_BORN_MAGIC_WIND";
}
