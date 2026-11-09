using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.DerivativeNemo;

/// <summary>
///     虚数脱出: 让出牌者本人脱离〔虚数空间〕，并把攒下的探索点数换成实打实的战果。
/// </summary>
/// <remarks>
///     探索点数兑换（永久宝具值 /〔好似飞鸟〕）见 <see cref="GreatVoidSeaBattle.ConsumeExplorationPoints" />。
///     本卡是 <c>TargetType.Self</c> ⇒ <c>cardPlay.Target</c> 恒为 null，不要对它做非空断言。
/// </remarks>
[RegisterCard(typeof(TokenCardPool))]
public class VoidEscape() : FgoBaseCardModel(0, CardType.Skill,
    CardRarity.Token, TargetType.Self,
    false)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipFactory.FromVoidSpace(),
        FgoHoverTipFactory.FromExploration()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 探索点数按玩家挂在 Creature 上、兑换结果写进该玩家的局内存档，只结算出牌者本人。
        await GreatVoidSeaBattle.ConsumeExplorationPoints(choiceContext, Owner);

        // 只让出牌者退出：其余玩家的虚数空间状态、额外手牌与背景都不受影响。
        await FgoVoidHand.Exit(Owner);
    }
}