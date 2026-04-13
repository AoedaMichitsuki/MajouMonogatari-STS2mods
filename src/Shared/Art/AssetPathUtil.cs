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
        var name = dotIndex < 0 ? entryId : entryId[(dotIndex + 1)..];
        return name.Trim().ToLowerInvariant();
    }
}
