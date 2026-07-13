using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class FromTheWorldsEndPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/EveryTurnPower.png",
        "res://Fgo/images/powers/big/EveryTurnPower.png"
    );

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner)) return;
        if (side == CombatSide.Player)
        {
            var combatState = Owner.CombatState;
            if (combatState != null)
            {
                Flash();
                foreach (var enemy in combatState.HittableEnemies)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -1m, Owner, null);
                    await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 1m, Owner, null);
                }
            }

            await PowerCmd.Decrement(this);
        }
    }
}