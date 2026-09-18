using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Fgo.Scripts.Cards.DerivativeNemo;

/// <summary>
///     航线规划: 拆掉 1 层〔水边〕换取 1 点探索点数；打出后回到手牌。
/// </summary>
/// <remarks>
///     「打出后回到手牌」靠重写 <see cref="GetResultLocationForCardPlay" /> 把结果牌堆从弃牌堆改成
///     〔虚数空间〕额外手牌堆（官方 <c>ParticleWall</c> 同款钩子）。**不能**在 OnPlay 里自己搬——
///     那时卡还在 Play 牌堆，出牌流程末尾会按 <c>Pile.Type == PileType.Play</c> 再搬一次。
/// </remarks>
public class RoutePlanning() : FgoCardModel(0, CardType.Skill, 
    CardRarity.Token, TargetType.Self,
    shouldShowInCardLibrary: false)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.Waterside.ToHoverTip(),
        FgoHoverTipFactory.FromExploration()
    ];

    protected override CardLocation GetResultLocationForCardPlay()
    {
        var location = base.GetResultLocationForCardPlay();
        if (location.pileType != PileType.Discard) return location;

        // 回到〔虚数空间〕的额外手牌区，**不能**回原版手牌: 这张卡只存在于那组手牌里，
        // 落进原版手牌（此时被 HUD 隐藏）就再也打不出来了，而且回合结束会被弃掉。
        // 牌堆缺失时退回原版手牌，至少不会把卡弄丢。
        location.pileType = CardPile.Get(FgoEnums.VoidHand, location.player) != null
            ? FgoEnums.VoidHand
            : PileType.Hand;
        return location;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 负层数等价于减少: FgoFieldState.Add 在结果 <= 0 时走移除分支。
        FgoField.Add(Owner.Creature.CombatState, FgoFieldId.Waterside, -1);
        await PowerCmd.Apply<ExplorationPointsPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
