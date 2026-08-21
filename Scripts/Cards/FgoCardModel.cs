using Fgo.Scripts.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

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
    // NP 增加逻辑已移至 FgoBattleHooks.BeforeCardPlayed 全局钩子，
}