namespace MajouMonogatari_STS2mods.Shared.Art;

public sealed class CecilyArtProvider
{
    private const string Root = "Assets/cecily";
    private static readonly string CharacterVisualScenePrimary = AssetPathUtil.ResPath(Root, "character/scenes/cecily_visual.tscn");
    private static readonly string CharacterVisualSceneFallback = AssetPathUtil.ResPath(Root, "scenes/cecily_visual.tscn");

    public static CecilyArtProvider Instance { get; } = new();

    private CecilyArtProvider() 
    {
    }

    public string CharacterVisualScenePath => FirstExistingOrEmpty(CharacterVisualScenePrimary, CharacterVisualSceneFallback);

    public string CharacterIconTexturePath => ExistingOrEmpty(AssetPathUtil.ResPath(Root, "character/ui/icon_texture.png"));

    public string CharacterSelectIconPath => ExistingOrEmpty(AssetPathUtil.ResPath(Root, "character/ui/char_select_icon.png"));

    public string CharacterSelectLockedIconPath => ExistingOrEmpty(AssetPathUtil.ResPath(Root, "character/ui/char_select_locked.png"));

    public string CharacterMapMarkerPath => ExistingOrEmpty(AssetPathUtil.ResPath(Root, "character/ui/map_marker.png"));

    public string CardBigEnergyIconPath => ExistingOrEmpty(AssetPathUtil.ResPath(Root, "character/ui/big_energy.png"));

    public string CardTextEnergyIconPath => ExistingOrEmpty(AssetPathUtil.ResPath(Root, "character/ui/text_energy.png"));

    public string GetCardPortraitPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "cards/portraits", filename));
    }

    public string GetCardMiniPortraitPath(string entryId)
    {
        return GetCardPortraitPath(entryId);
    }

    public string GetCardBetaPortraitPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "cards/beta", filename));
    }

    public string GetPowerSmallIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "powers/small", filename));
    }

    public string GetPowerBigIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "powers/big", filename));
    }

    public string GetRelicIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "relics", filename));
    }

    public string GetRelicOutlineIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + "_outline.png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "relics", filename));
    }

    public string GetBigRelicIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return ExistingOrEmpty(AssetPathUtil.ResPath(Root, "relics/big", filename));
    }

    private static string ExistingOrEmpty(string path)
    {
        return AssetPathUtil.ResolveOrFallback(path, string.Empty);
    }

    private static string FirstExistingOrEmpty(string first, string second)
    {
        var primary = ExistingOrEmpty(first);
        if (!string.IsNullOrWhiteSpace(primary))
        {
            return primary;
        }

        return ExistingOrEmpty(second);
    }
}
