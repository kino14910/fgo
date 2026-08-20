using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
        for (var i = 0; i < Amount; i++)
        {
            var cardModel = CardFactory.GetDistinctForCombat(player,
                from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState,
                    player.RunState.CardMultiplayerConstraint)
                where c.Type == CardType.Attack
                select c, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
            if (cardModel != null) await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, Owner.Player);
        }
    }
}