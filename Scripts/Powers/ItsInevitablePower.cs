using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class ItsInevitablePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/EveryTurnPower.png",
        "res://Fgo/images/powers/big/EveryTurnPower.png"
    );
    
    private decimal Boost
    {
        get => DynamicVars["Boost"].BaseValue;
        set
        {
            DynamicVars["Boost"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Boost", 0)
    ];
    
    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            // 右上角: 增长
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.TopRight, $"{Boost}%"),
            // 右下角: 层数
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.BottomRight, Amount.ToString())
        ];
    }

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (cardSource is ItsInevitable)
        {
            Boost += amount;
        }

        return base.BeforeApplied(target, amount, applier, cardSource);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        Flash();
        var combatState = Owner.CombatState;
        if (combatState != null)
            await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, Amount,
                ValueProp.Unpowered | ValueProp.Move, Owner);
        await PowerCmd.Apply<ItsInevitablePower>(choiceContext, Owner, Boost, Owner, null);
    }
}