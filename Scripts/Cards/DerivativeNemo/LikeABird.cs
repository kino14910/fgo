using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Fgo.Scripts.Cards.DerivativeNemo;

/// <summary>
///     好似飞鸟: 在〔虚数空间〕中造成的伤害变为 200%。
/// </summary>
/// <remarks>
///     仅当玩家解锁过（<see cref="GreatVoidSeaBattle.IsLikeABirdUnlocked" />）时，
///     加入和移除逻辑由 <see cref="FgoVoidHand" /> 管理。
/// </remarks>
public class LikeABird() : FgoCardModel(0, CardType.Power, 
    CardRarity.Token, TargetType.Self,
    shouldShowInCardLibrary: false)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.ImaginarySpace.ToHoverTip(),
        HoverTipFactory.FromPower<LikeABirdPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LikeABirdPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
