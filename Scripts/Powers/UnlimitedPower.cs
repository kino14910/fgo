using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class UnlimitedPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/EveryTurnPower.png",
        "res://Fgo/images/powers/big/EveryTurnPower.png"
    );

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner.Player) return;

        var candidates = player.Character.CardPool.GetUnlockedCards(player.UnlockState,
                player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Attack)
            .ToList();
        if (candidates.Count == 0) return;

        var cards = new CardModel[Amount];
        var rng = player.RunState.Rng.CombatCardGeneration;
        for (var i = 0; i < Amount; i++)
            CardCmd.ApplyKeyword(
                cards[i] = CardFactory.GetDistinctForCombat(player, candidates, 1, rng).First(),
                CardKeyword.Exhaust);

        Flash();
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner.Player);
    }
}