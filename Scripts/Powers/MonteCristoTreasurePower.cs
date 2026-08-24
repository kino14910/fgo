using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class MonteCristoTreasurePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/TriggerAfterAttacksPower.png",
        "res://Fgo/images/powers/big/TriggerAfterAttacksPower.png"
    );

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner) return;
        if (!props.IsCardOrMonsterMove()) return;
        if (Owner?.Player is not { } player || !FgoBattleHooks.Get(player).CritTriggered) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, result.TotalDamage, ValueProp.Unpowered, null);
    }
}