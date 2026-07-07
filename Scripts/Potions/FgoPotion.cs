using MegaCrit.Sts2.Core.Models.PotionPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Potions;

[RegisterPotion(typeof(SharedPotionPool), Inherit = true)]
public abstract class FgoPotionModel : ModPotionTemplate;