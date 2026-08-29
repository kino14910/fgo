using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

public class SquireOfProphecyPower : FgoPowerModel
{
    private bool _triggeredThisTurn;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (_triggeredThisTurn) return;
        if (Owner.Player?.PlayerCombatState is null) return;
        if (!Owner.Player.PlayerCombatState.Energy.Equals(0)) return;
        
        Flash();
        await PlayerCmd.GainEnergy(2, Owner.Player);
        _triggeredThisTurn = true;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (side != CombatSide.Player) return;

        await PowerCmd.Decrement(this);
        _triggeredThisTurn = false;
    }
}