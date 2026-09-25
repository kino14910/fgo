using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     回转膳食（Kurakura's Meals）: 多人专用能力。打出时先给一张〔炒荞麦面〕，
///     之后每回合开始轮流发放一张菜品卡；菜品卡会附带暴击星与饱腹，
///     饱腹满后菜品卡完全空转（见 <see cref="FullnessPower" />）。
/// </summary>
public class KurakurasMeals() : FgoCardModel(0, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<KurakurasMealsPower>(),
        HoverTipFactory.FromPower<FullnessPower>(),
        FgoHoverTipFactory.FromStar()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Star", 20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Star"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<KurakurasMealsPower>(choiceContext, Owner.Creature, 1m,
            Owner.Creature, this);
        if (power == null) return;

        power.Configure(DynamicVars["Star"].BaseValue, IsUpgraded);

        // 首张菜品卡在打出当回合就发，轮换位置从第 2 张（咖喱）继续。
        await KurakurasMealsPower.GrantNextDish(choiceContext, power, Owner);
    }
}
