using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards;

[RegisterCard(typeof(ColorlessCardPool), Inherit = true)]
public abstract class FgoColorlessCard(
    int cost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool shouldShowInCardLibrary = true)
    : FgoCardBase(cost, type, rarity, target, shouldShowInCardLibrary)
{
}