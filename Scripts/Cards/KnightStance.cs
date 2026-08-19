using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class KnightStance() : FgoCardModel(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ReducePercentDamagePower>(),
        HoverTipFactory.FromPower<NpRatePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Block(8),
        ModCardVars.Power<ReducePercentDamagePower>(20)
    ];

    public override bool GainsBlock => true;

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}