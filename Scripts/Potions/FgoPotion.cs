using Fgo.Scripts.Character;
using MegaCrit.Sts2.Core.Models.PotionPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Potions;

[RegisterPotion(typeof(FgoPotionPool), Inherit = true)]
public abstract class FgoPotionModel : ModPotionTemplate
{
    public override PotionAssetProfile AssetProfile => new(
        $"res://Fgo/images/potions/{GetType().Name}.png"
    );
}