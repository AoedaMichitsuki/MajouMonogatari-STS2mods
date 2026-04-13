using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MajouMonogatari_STS2mods.Shared.Art;

namespace MajouMonogatari_STS2mods.Characters.Cecily;

[CustomID(CecilyIds.PotionPool)]
public class CecilyPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => CecilyCharacter.CecilyColor;
    public override string BigEnergyIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CardBigEnergyIconPath, base.BigEnergyIconPath);

    public override string TextEnergyIconPath =>
        AssetPathUtil.ResolveOrFallback(CecilyArtProvider.Instance.CardTextEnergyIconPath, base.TextEnergyIconPath);
}
