using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>咖喱: 所有友方获得 3(6) 点额外最大生命值。</summary>
public class Curry : FgoDishCardModel
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<MaxHpPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<MaxHpPower>(3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(3);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<MaxHpPower>(choiceContext, Allies,
            DynamicVars[nameof(MaxHpPower)].BaseValue, Owner.Creature, this);
    }
}
