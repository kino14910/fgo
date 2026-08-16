using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class BurningPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        var combatState = Owner.CombatState;
        if (combatState != null)
            await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, Amount,
                ValueProp.Unpowered | ValueProp.Move, Owner);
        await PowerCmd.Apply<BurningPower>(choiceContext, Owner, Amount, Owner, null);
    }
}