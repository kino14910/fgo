using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     空前绝后: 多人专用。自己爆费并把宝具威力与手牌一次性拉满，
///     代价是本回合内其他玩家不能再抽任何牌。
/// </summary>
public class UltraManifesto() : FgoCardModel(4, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<NoDrawPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NpDamagePower>(30)
    ];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(8, Owner);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);

        // 先把自己的手牌抽满，再封锁盟友；顺序不能反——NoDrawPower 只拦"之后"的抽牌。
        var missing = CardPile.MaxCardsInHand - PileType.Hand.GetPile(Owner).Cards.Count;
        if (missing > 0)
            await CardPileCmd.Draw(choiceContext, missing, Owner);

        var allies = CombatState!.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != Owner.Creature);
        await PowerCmd.Apply<NoDrawPower>(choiceContext, allies, 1m, Owner.Creature, this);
    }
}
