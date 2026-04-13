using System;
using Godot;

namespace MajouMonogatari_STS2mods.Shared.ArtController;

/// <summary>
/// 资源路径辅助工具：
/// - 统一做路径拼接与兜底。
/// - 避免业务代码到处写 ResourceLoader.Exists 与字符串拼接。
/// </summary>
public static class 
    AssetPathUtil
{
    /// <summary>
    /// 将多个路径段拼成 Godot 资源路径（res://...）。
    /// </summary>
    public static string ResPath(params string[] parts)
    {
        var safe = string.Join('/', parts).Replace("\\", "/").TrimStart('/');
        return $"res://{safe}";
    }

    /// <summary>
    /// 如果主路径存在则返回主路径，否则返回回退路径。
    /// </summary>
    public static string ResolveOrFallback(string primaryPath, string fallbackPath)
    {
        if (!string.IsNullOrWhiteSpace(primaryPath) && ResourceLoader.Exists(primaryPath))
        {
            return primaryPath;
        }

        return fallbackPath;
    }

    /// <summary>
    /// 把模型 EntryId 标准化为文件名。
    /// 示例："majoumonogatari-sts2mods.cecily.wind_bullet" -> "wind_bullet"
    /// </summary>
    public static string NormalizeEntryId(string entryId)
    {
        if (string.IsNullOrWhiteSpace(entryId))
        {
            return "unknown";
        }

        var lastDot = entryId.LastIndexOf('.');
        var name = lastDot >= 0 ? entryId[(lastDot + 1)..] : entryId;
        return name.Trim().ToLowerInvariant();
    }
}
