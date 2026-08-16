using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class SpringOfFirePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/TriggerAfterGutsPowerPower.png",
        "res://Fgo/images/powers/big/TriggerAfterGutsPowerPower.png"
    );

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power is GutsPower or NonStackableGutsPower)
        {
            Flash();
            await PowerCmd.Apply<NpDamagePower>(choiceContext, cardSource?.GetTargets(), Amount, applier, cardSource);
        }
    }
}