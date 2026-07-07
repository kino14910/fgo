using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Fragarach : NobleCardModel
{
    public Fragarach() : base(1, CardType.Power, TargetType.Self)
    {
        WithDamage(15, 5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<FragarachPower>(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue,
            Owner.Creature, this);
    }
}