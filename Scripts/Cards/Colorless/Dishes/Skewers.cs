using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>烤串 / Skewers: 所有友方获得 30(50)% 暴击威力。</summary>
public class Skewers : FgoDishCardModel
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<CriticalDamagePower>(30)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(CriticalDamagePower)].UpgradeValueBy(20);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Allies,
            DynamicVars[nameof(CriticalDamagePower)].BaseValue, Owner.Creature, this);
    }
}
