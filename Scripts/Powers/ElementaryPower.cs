using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class ElementaryPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();

        var enemies = Owner.CombatState?.HittableEnemies;
        if (enemies == null) return;

        foreach (var enemy in enemies)
        {
            if (enemy.Block <= 0) continue;
            var blockAmount = enemy.Block;
            await CreatureCmd.LoseBlock(enemy, blockAmount);
            await CreatureCmd.Damage(choiceContext, enemy, blockAmount,
                ValueProp.Unpowered, Owner);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, Amount, Owner, null);
        }
    }
}