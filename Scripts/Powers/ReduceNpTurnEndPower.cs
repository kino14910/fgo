using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class ReduceNpTurnEndPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/AfterDurationDebuffPower.png",
        "res://Fgo/images/powers/big/AfterDurationPower.png"
    );
    
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner?.Player is not { } player) return;
        await FgoBattleHooks.Get(player).ModifyNp(-Amount);
        await PowerCmd.Remove(this);
    }
}