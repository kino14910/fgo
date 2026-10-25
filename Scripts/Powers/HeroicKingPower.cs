using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class HeroicKingPower : FgoPowerModel
{
    private int _lastCritActivation;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner) return;
        if (Owner.Player is not { } player) return;
        if (result.TotalDamage <= 0) return;

        var state = FgoBattleHooks.Get(player);
        if (!state.CritTriggered) return;
        // 多段攻击的每一段都会调 AfterDamageGiven，按暴击激活编号去重，保证一次暴击只加一层
        if (state.CritActivationId == _lastCritActivation) return;
        _lastCritActivation = state.CritActivationId;

        Flash();
        await PowerCmd.Apply<HeroicKingPower>(choiceContext, Owner, 1m, Owner, cardSource);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.TotalDamage <= 0) return;

        Flash();
        await PowerCmd.Apply<HeroicKingPower>(choiceContext, Owner, 1m, Owner, cardSource);
    }
}