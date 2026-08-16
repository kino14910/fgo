using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

[RegisterCard(typeof(ColorlessCardPool), Inherit = true)]
public abstract class FgoColorlessCardModel(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : FgoBaseCardModel(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
}