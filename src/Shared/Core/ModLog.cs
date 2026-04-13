using System;
using Godot;

namespace MajouMonogatari_STS2mods.Shared.Core;

public static class ModLog
{
    public static string Prefix { get; set; } = $"[{ModEntry.ModConstants.ModId}]";

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
