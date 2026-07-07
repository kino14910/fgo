using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class WatersidePower : FgoPowerModel
{
    private const decimal BlockAmount = 3m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        Flash();

        var combatState = Owner.CombatState;
        if (combatState == null) return;

        await CreatureCmd.GainBlock(Owner, BlockAmount, ValueProp.Unpowered, null);

        foreach (var enemy in combatState.HittableEnemies)
            await CreatureCmd.GainBlock(enemy, BlockAmount, ValueProp.Unpowered, null);
    }
}