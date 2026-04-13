using System.Runtime.CompilerServices;
using Godot;

namespace MajouMonogatari_STS2mods.ModEntry;

/// <summary>
/// Mod 启动入口。
/// 双入口设计：
/// - ModuleInitializer：尽早初始化（优先覆盖纯 C# 入口）。
/// - Node._Ready：在 Godot 节点实际进入场景树时兜底初始化。
/// </summary>
public partial class ModEntry : Node
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void ModuleInit()
    {
        ModBootstrap.InitializeOnce("module");
    }

    public override void _Ready()
    {
        ModBootstrap.InitializeOnce("node-ready");
    }
}
