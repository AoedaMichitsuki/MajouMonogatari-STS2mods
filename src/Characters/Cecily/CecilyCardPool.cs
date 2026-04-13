using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MajouMonogatari_STS2mods.Shared.Art;

namespace MajouMonogatari_STS2mods.Characters.Cecily;

[CustomID(CecilyIds.CardPool)]
public class CecilyCardPool : CustomCardPoolModel
{
    public override string Title => CecilyIds.Character;

    public override float H => 0.53f;
    public override float S => 0.78f;
    public override float V => 0.92f;

    public override Color DeckEntryCardColor => new("2d7298");
    public override Color EnergyOutlineColor => new("1e4a63");
    public override bool IsColorless => false;

    public override string BigEnergyIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CardBigEnergyIconPath, base.BigEnergyIconPath);

    public override string TextEnergyIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CardTextEnergyIconPath, base.TextEnergyIconPath);
}
