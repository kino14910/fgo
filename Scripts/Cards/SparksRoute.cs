using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Fgo.Scripts.Cards;

/// <summary>
///     火花路径∞: 失去至少 10% 宝具值；把手牌升级后<b>放回抽牌堆</b>（不是丢弃），然后抽相同数量张牌。
/// </summary>
public class SparksRoute() : FgoCardModel(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipFactory.FromNp()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 失去至少 10% 宝具值（向上取整，至少 1 点）。
        var state = FgoBattleHooks.Get(Owner);
        var loss = Math.Max(1, (int)Math.Ceiling(state.Np * 0.1m));
        await FgoResCmd.ModifyNp(-loss, Owner);

        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count == 0) return;

        var toMove = hand;

        if (IsUpgraded)
        {
            // 「任意数量」= 0 ~ 手牌数都合法，由玩家自选。
            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, hand.Count);
            toMove = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).ToList();

            if (toMove.Count == 0) return;
        }

        // 2. 升级（只对仍可升级的卡生效）。
        var upgraded = toMove.Where(card => card.IsUpgradable).ToList();
        foreach (var card in upgraded)
            CardCmd.Upgrade(card, CardPreviewStyle.None);

        await CardPileCmd.Add(toMove, PileType.Draw.GetPile(Owner), CardPilePosition.Random);

        await CardPileCmd.Draw(choiceContext, toMove.Count, Owner);
    }
}
