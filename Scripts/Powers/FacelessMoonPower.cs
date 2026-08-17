using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class FacelessMoonPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldFlush(Player player) => player != Owner.Player;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (side != CombatSide.Player) return;
        if (Owner.Player?.PlayerCombatState is null) return;
        
        Flash();
        await CreatureCmd.GainBlock(Owner, Owner.Player.PlayerCombatState.Hand.Cards.Count, ValueProp.Unpowered, null);
        await FgoResCmd.ModifyStars(Amount, Owner.Player);
        await PowerCmd.Remove(this);
    }
}