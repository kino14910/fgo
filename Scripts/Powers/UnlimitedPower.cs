using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
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

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Player.Creature)) return;
        IReadOnlyList<CardModel> readOnlyList =
            (from c in Owner.Player.Character.CardPool.GetUnlockedCards(Owner.Player.UnlockState,
                    Owner.Player.RunState.CardMultiplayerConstraint)
                where c.Type == CardType.Attack
                select c).ToList();
        if (readOnlyList.Count == 0) return;
        Flash();
        var list = CardFactory
            .GetDistinctForCombat(Owner.Player, readOnlyList, 1, Owner.Player.RunState.Rng.CombatCardGeneration)
            .ToList();
        foreach (var item in list)
        {
            item.SetToFreeThisTurn();
            item.AddKeyword(CardKeyword.Exhaust);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, Owner.Player);
    }
}