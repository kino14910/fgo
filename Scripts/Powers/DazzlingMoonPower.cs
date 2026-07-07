using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class DazzlingMoonPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/EveryTurnPower.png",
        "res://Fgo/images/powers/big/EveryTurnPower.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -1m, Owner, null);
        var combatState = Owner.CombatState;
        if (combatState != null)
            foreach (var enemy in combatState.HittableEnemies)
                await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -2m, Owner, null);

        await PowerCmd.Decrement(this);
    }

    public override async Task AfterRemoved(Creature owner)
    {
        var str = owner.GetPowerAmount<StrengthPower>();
        if (str < 0)
        {
            var abs = -str;
            await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), Owner, abs * 2m, Owner, null);
            await PowerCmd.Apply<DexterityPower>(new BlockingPlayerChoiceContext(), Owner, abs, Owner, null);
        }
    }
}