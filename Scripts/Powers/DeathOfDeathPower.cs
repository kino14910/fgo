using Fgo.Scripts.Cards;
using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class DeathOfDeathPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/TriggerAfterAttacksPower.png",
        "res://Fgo/images/powers/big/TriggerAfterAttacksPower.png"
    );


    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Owner || Amount <= 0) return;

        Flash();
        await ReviveCmd.Execute(creature, Amount);
        await PowerCmd.Apply<ShadowStepPower>(
            new BlockingPlayerChoiceContext(), Owner, 1m, Owner, ModelDb.Card<AbyssOfDeath>());
    }
}