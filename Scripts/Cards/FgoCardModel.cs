using Fgo.Scripts.Character;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

[RegisterCard(typeof(FgoCardPool), Inherit = true)]
public abstract class FgoCardModel(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : FgoBaseCardModel(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    // NP 增加逻辑已移至 FgoPlayerResources.BeforeCardPlayed 全局钩子，
}