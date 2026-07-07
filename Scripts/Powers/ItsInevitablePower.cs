using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class ItsInevitablePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int Boost { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<ItsInevitablePower>(choiceContext, Owner, Boost, Owner, null);
        var combatState = Owner.CombatState;
        if (combatState != null)
            foreach (var enemy in combatState.HittableEnemies)
                await CreatureCmd.Damage(choiceContext, enemy, Amount,
                    ValueProp.Unpowered | ValueProp.Move, Owner);
    }
}