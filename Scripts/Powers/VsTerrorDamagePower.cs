using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class VsTerrorDamagePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/SpDmgUpPower.png",
        "res://Fgo/images/powers/big/SpDmgUpPower.png"
    );

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner != dealer) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null) return 1m;
        if (!target.HasPower<TerrorPower>()) return 1m;
        return (100m + Amount) / 100m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (side != CombatSide.Player) return;

        await PowerCmd.Remove(this);
    }
}