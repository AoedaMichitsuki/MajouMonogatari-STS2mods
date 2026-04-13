using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace MajouMonogatari_STS2mods.ModEntry;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ResPath = $"res://{ModConstants.ModId}";

    public static void Initialize()
    {
        ModBootstrap.Initialize();
    }

    public override void _Ready()
    {
        ModBootstrap.Initialize();
    }
}
