using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class ForeignerCriticalDamagePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner != dealer
            || !props.IsPoweredAttack()
            || Owner?.Player is not { } player
            || !FgoBattleHooks.Get(player).WillCritOnPlay(cardSource, cardPlay == null)
            || (cardSource?.Tags.Any(c => c == FgoTags.Foreigner) ?? false))
            return 0m;
        return amount * Amount / 100m;
    }
}