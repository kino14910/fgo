using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     巨型规模：打出后获得 HugeScalePower。
///     在每个回合开始获得额外最大生命值，并将一张幼儿退化加入手牌。
/// </summary>
public class HugeScale() : FgoCardModel(2, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<MaxHpPower>(),
        HoverTipFactory.FromCard<InfantileRegression>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<MaxHpPower>(6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FgoCardActions.AddToPile(CombatState!.CreateCard(
            ModelDb.Card<InfantileRegression>(), Owner), PileType.Hand);
        // HugeScalePower.Amount 即每回合获得的额外最大生命值（与卡上显示的 MaxHpPower 一致）。
        await PowerCmd.Apply<HugeScalePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(MaxHpPower)].BaseValue, Owner.Creature, this);
    }
}