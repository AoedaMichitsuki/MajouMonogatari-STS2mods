using Godot;

namespace MajouMonogatari_STS2mods.Shared.Art;

public static class AssetPathUtil
{
    public static string ResPath(params string[] parts)
    {
        var normalized = string.Join('/', parts)
            .Replace("\\", "/")
            .TrimStart('/');
        return $"res://{normalized}";
    }

    public static string ResolveOrFallback(string primaryPath, string fallbackPath)
    {
        if (!string.IsNullOrWhiteSpace(primaryPath) && ResourceLoader.Exists(primaryPath))
        {
            return primaryPath;
        }

        return fallbackPath;
    }

    public static string NormalizeEntryId(string entryId)
    {
        if (string.IsNullOrWhiteSpace(entryId))
        {
            return "placeholder";
        }

        var dotIndex = entryId.LastIndexOf('.');
        var raw = dotIndex < 0 ? entryId : entryId[(dotIndex + 1)..];

        const string cecilyPrefix = "MAJOUMONOGATARI_STS2MODS_CECILY_";
        var name = raw.StartsWith(cecilyPrefix) ? raw[cecilyPrefix.Length..] : raw;
        return name.Trim().ToLowerInvariant();
    }
}
