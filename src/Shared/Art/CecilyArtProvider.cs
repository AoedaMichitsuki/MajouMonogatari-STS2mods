namespace MajouMonogatari_STS2mods.Shared.Art;

public sealed class CecilyArtProvider
{
    private const string Root = "Assets/cecily";
    private const string CardPlaceholder = "cards/portraits/placeholder.png";
    private const string CardBetaPlaceholder = "cards/beta/placeholder.png";
    private const string UiIconPlaceholder = "character/ui/placeholder_icon.png";
    private const string UiSelectPlaceholder = "character/ui/placeholder_select.png";
    private const string UiSelectLockedPlaceholder = "character/ui/placeholder_select_locked.png";
    private const string UiMapMarkerPlaceholder = "character/ui/placeholder_map_marker.png";
    private const string UiBigEnergyPlaceholder = "character/ui/placeholder_big_energy.png";
    private const string UiTextEnergyPlaceholder = "character/ui/placeholder_text_energy.png";
    private const string PowerSmallPlaceholder = "powers/small/placeholder.png";
    private const string PowerBigPlaceholder = "powers/big/placeholder.png";

    public static CecilyArtProvider Instance { get; } = new();

    private CecilyArtProvider()
    {
    }

    public string CharacterVisualScenePath => AssetPathUtil.ResPath(Root, "character/scenes/cecily_visual.tscn");

    public string CharacterIconTexturePath =>
        AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "character/ui/icon_texture.png"),
            AssetPathUtil.ResPath(Root, UiIconPlaceholder));

    public string CharacterSelectIconPath =>
        AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "character/ui/char_select_icon.png"),
            AssetPathUtil.ResPath(Root, UiSelectPlaceholder));

    public string CharacterSelectLockedIconPath =>
        AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "character/ui/char_select_locked.png"),
            AssetPathUtil.ResPath(Root, UiSelectLockedPlaceholder));

    public string CharacterMapMarkerPath =>
        AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "character/ui/map_marker.png"),
            AssetPathUtil.ResPath(Root, UiMapMarkerPlaceholder));

    public string CardBigEnergyIconPath =>
        AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "character/ui/big_energy.png"),
            AssetPathUtil.ResPath(Root, UiBigEnergyPlaceholder));

    public string CardTextEnergyIconPath =>
        AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "character/ui/text_energy.png"),
            AssetPathUtil.ResPath(Root, UiTextEnergyPlaceholder));

    public string GetCardPortraitPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "cards/portraits", filename),
            AssetPathUtil.ResPath(Root, CardPlaceholder));
    }

    public string GetCardMiniPortraitPath(string entryId)
    {
        return GetCardPortraitPath(entryId);
    }

    public string GetCardBetaPortraitPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        var betaPath = AssetPathUtil.ResPath(Root, "cards/beta", filename);
        return AssetPathUtil.ResolveOrFallback(betaPath, AssetPathUtil.ResPath(Root, CardBetaPlaceholder));
    }

    public string GetPowerSmallIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "powers/small", filename),
            AssetPathUtil.ResPath(Root, PowerSmallPlaceholder));
    }

    public string GetPowerBigIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "powers/big", filename),
            AssetPathUtil.ResPath(Root, PowerBigPlaceholder));
    }

    public string GetRelicIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "relics", filename),
            AssetPathUtil.ResPath("icon.svg"));
    }

    public string GetRelicOutlineIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + "_outline.png";
        return AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "relics", filename),
            GetRelicIconPath(entryId));
    }

    public string GetBigRelicIconPath(string entryId)
    {
        var filename = AssetPathUtil.NormalizeEntryId(entryId) + ".png";
        return AssetPathUtil.ResolveOrFallback(
            AssetPathUtil.ResPath(Root, "relics/big", filename),
            GetRelicIconPath(entryId));
    }
}
