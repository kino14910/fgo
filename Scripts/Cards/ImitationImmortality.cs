using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class ImitationImmortality() : FgoCardModel(2, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GutsPower>(),
        HoverTipFactory.FromPower<RegenPower>(),
        HoverTipFactory.FromPower<NpPerTurnPower>(),
        HoverTipFactory.FromPower<ReducePercentDamagePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<GutsPower>(5),
        ModCardVars.Power<RegenPower>(3),
        ModCardVars.Power<NpPerTurnPower>(10),
        ModCardVars.Power<ReducePercentDamagePower>(10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(GutsPower)].UpgradeValueBy(5);
        DynamicVars[nameof(RegenPower)].UpgradeValueBy(2);
        DynamicVars[nameof(NpPerTurnPower)].UpgradeValueBy(5);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GutsPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(GutsPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(RegenPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpPerTurnPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
    }
}