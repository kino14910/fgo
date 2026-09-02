using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     相位滑剑: 每打出一张牌，从抽牌堆随机取一张与此牌不同类型(CardType)的牌加入手牌，
///     共触发 Amount 次。
/// </summary>
public class PhaseGlidingBladePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/TriggerAfterAttacksPower.png",
        "res://Fgo/images/powers/big/TriggerAfterAttacksPower.png"
    );

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is PhaseGlidingBlade) return;

        if (Owner.Player is not { } player) return;

        Flash();

        // 仿照 Catastrophe: 从抽牌堆随机取一张与此牌类型不同的可打牌加入手牌。
        // 每轮实时基于当前抽牌堆选取，避免取到已移走的牌。
        for (var i = 0; i < Amount; i++)
        {
            var card = PileType.Draw.GetPile(player).Cards
                .Where(c => c.Type != cardPlay.Card.Type)
                .ToList()
                .StableShuffle(player.RunState.Rng.Shuffle)
                .FirstOrDefault();
            if (card == null) break;

            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top);
        }
    }
}