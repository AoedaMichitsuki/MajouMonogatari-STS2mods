using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MajouMonogatari_STS2mods.Characters.Cecily.Relics;

[CustomID(CecilyIds.BornMagicWindRelic)]
public class BornMagicWindRelic : CecilyRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
