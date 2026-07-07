using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class AbyssOfDeath : FgoCardModel
{
    public AbyssOfDeath() : base(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithPower<RegenPower>(3, 2);
        WithPower<DeathOfDeathPower>(5, 3);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[typeof(RegenPower).Name].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<DeathOfDeathPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(DeathOfDeathPower).Name].BaseValue, Owner.Creature, this);
    }
}