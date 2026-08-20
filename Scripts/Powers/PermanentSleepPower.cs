using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class PermanentSleepPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer == Owner && props.IsCardOrMonsterMove() && cardSource is NobleCardModel) return 2m;

        return 1m;
    }

    public override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner.Player?.PlayerCombatState is null) return;

        var pile = PileType.Hand.GetPile(Owner.Player);
        var cardModel = Owner.Player.RunState.Rng.CombatCardSelection.NextItem(pile.Cards);
        if (cardModel != null) await CardCmd.Exhaust(choiceContext, cardModel);
    }
}