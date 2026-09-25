using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>法兰克福香肠 / Frankfurt: 所有友方获得 10(20)% 宝具威力。</summary>
public class Frankfurt : FgoDishCardModel
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NpDamagePower>(10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NpDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Allies,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
    }
}
