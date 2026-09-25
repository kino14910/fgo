using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>炒荞麦面: 所有友方获得 1(2) 点力量。也是〔回转膳食〕轮换的起点与变异对象。</summary>
public class Yakisoba : FgoDishCardModel
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<StrengthPower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Allies,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
    }
}
