using System;
using Godot;

namespace MajouMonogatari_STS2mods.Shared.Core;

/// <summary>
/// 统一日志输出工具。
/// 设计目标：
/// - 在 Godot 运行时写入 Godot 控制台。
/// - 在非 Godot 环境（测试/工具）自动回退到标准输出。
/// </summary>
public static class ModLog
{
    /// <summary>
    /// 日志前缀，通常在初始化时按 ModId 设置。
    /// </summary>
    public static string Prefix { get; set; } = "[majoumonogatari-sts2mods]";

    public static void Info(string message)
    {
        Write(GD.Print, "INFO", message);
    }

    public static void Warn(string message)
    {
        Write(GD.PushWarning, "WARN", message);
    }

    public static void Error(string message)
    {
        Write(GD.PushError, "ERROR", message);
    }

    private static void Write(Action<string> godotWriter, string level, string message)
    {
        var line = $"{Prefix} [{level}] {message}";

        try
        {
            godotWriter(line);
        }
        catch
        {
            Console.WriteLine(line);
        }
    }
}
