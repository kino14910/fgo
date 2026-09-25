using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>拉面: 所有友方获得 10(20)% 宝具值。</summary>
public class Ramen : FgoDishCardModel
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipFactory.FromNp()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Np", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        await GrantNpToAllies(DynamicVars["Np"].BaseValue);
    }
}
