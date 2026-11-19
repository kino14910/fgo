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
    // 多段命中共享同一次暴击的 Id，只在该次激活的首次命中叠一层，靠它去重。
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