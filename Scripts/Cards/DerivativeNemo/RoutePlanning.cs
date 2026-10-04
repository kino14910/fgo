using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.DerivativeNemo;

/// <summary>
///     航线规划: 拆掉 1 层〔水边〕换取 1 点探索点数；打出后回到手牌。
/// </summary>
/// <remarks>
///     「打出后回到手牌」靠重写 <see cref="GetResultLocationForCardPlay" /> 把结果牌堆从弃牌堆改成
///     〔虚数空间〕额外手牌堆（官方 <c>ParticleWall</c> 同款钩子）。**不能**在 OnPlay 里自己搬——
///     那时卡还在 Play 牌堆，出牌流程末尾会按 <c>Pile.Type == PileType.Play</c> 再搬一次。
/// </remarks>
[RegisterCard(typeof(TokenCardPool))]
public class RoutePlanning() : FgoBaseCardModel(0, CardType.Skill, 
    CardRarity.Token, TargetType.Self,
    shouldShowInCardLibrary: false)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.Waterside.ToHoverTip(),
        FgoHoverTipFactory.FromExploration()
    ];

    protected override bool IsPlayable => FgoField.Has(CombatState, FgoFieldId.Waterside);

    protected override CardLocation GetResultLocationForCardPlay()
    {
        var location = base.GetResultLocationForCardPlay();
        if (location.pileType != PileType.Discard) return location;

        location.pileType = CardPile.Get(FgoEnums.VoidHand, location.player) != null
            ? FgoEnums.VoidHand
            : PileType.Hand;
        return location;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 负层数等价于减少: FgoFieldState.Add 在结果 <= 0 时走移除分支。
        await FgoField.Add(Owner.Creature.CombatState, FgoFieldId.Waterside, -1);
        await PowerCmd.Apply<ExplorationPointsPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
