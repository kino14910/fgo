using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Fields;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Fgo.Scripts.Cards.DerivativeNemo;

/// <summary>
///     虚数脱出: 脱离〔虚数空间〕，并把在虚数空间里攒下的探索点数换成实打实的战果。
/// </summary>
/// <remarks>
///     结算顺序刻意是「先大冲角、后退出、最后结算探索点数」: <see cref="GreatRamNautilus" />
///     结算时读〔虚数空间〕拿 1.5 倍伤害加成，故退出必须排在它之后。
///     探索点数兑换（永久宝具值 /〔好似飞鸟〕）见 <see cref="GreatVoidSeaBattle.ConsumeExplorationPoints" />。
///     本卡是 <c>TargetType.Self</c> ⇒ <c>cardPlay.Target</c> 恒为 null，不要对它做非空断言。
/// </remarks>
public class VoidEscape() : FgoCardModel(0, CardType.Skill, 
    CardRarity.Token, TargetType.Self,
    shouldShowInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.ImaginarySpace.ToHoverTip(),
        FgoHoverTipFactory.FromExploration()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        await GreatVoidSeaBattle.ConsumeExplorationPoints(choiceContext, Owner);

        await FgoField.Remove(combat, FgoFieldId.ImaginarySpace);
        await FgoVoidHand.ExitAll(combat);
    }
}
