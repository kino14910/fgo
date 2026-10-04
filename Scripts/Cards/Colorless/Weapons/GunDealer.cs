using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.Weapons;

/// <summary>
///     军火经销商: 0 费、保留。打出时从四张武器牌中选一张直接加入手牌。
/// </summary>
/// <remarks>
///     四个候选都由 <c>CombatState.CreateCard</c> 从 canonical 生成 —— 数量与顺序各端完全一致，
///     因此 <c>CardSelectCmd.FromSimpleGrid</c> 的"按 index 跨端同步"是安全的。
///     <para />
///     未被选中的候选从未进过任何牌堆（<c>Pile == null</c>），因此**不能**用
///     <c>CardPileCmd.RemoveFromCombat</c> 清理（它会因 "Card must be in a combat pile" 抛异常）；
///     正确做法是 <c>CombatState.RemoveCard</c> 把它从 <c>_allCards</c> 摘掉，再置
///     <c>HasBeenRemovedFromState</c>，免得每次打出都留下 3 张废卡。
/// </remarks>
[RegisterCard(typeof(TokenCardPool))]
public class GunDealer() : FgoBaseCardModel(0, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Pistol>(),
        HoverTipFactory.FromCard<SMG>(),
        HoverTipFactory.FromCard<SniperRifle>(),
        HoverTipFactory.FromCard<RocketLauncher>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        var options = new List<CardModel>
        {
            combat.CreateCard<Pistol>(Owner),
            combat.CreateCard<SMG>(Owner),
            combat.CreateCard<SniperRifle>(Owner),
            combat.CreateCard<RocketLauncher>(Owner)
        };


        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, options, Owner, new CardSelectorPrefs(
            SelectionScreenPrompt, 1
        ))).FirstOrDefault();

        FgoCardActions.DiscardUnpiledCandidates(combat, options, selected);

        if (selected == null) return;

        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
    }
}