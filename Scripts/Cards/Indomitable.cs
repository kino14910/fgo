using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class Indomitable : FgoCardModel
{
    public Indomitable() : base(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithPower<NonStackableGutsPower>(3, 2);
        WithPower<IndomitablePower>(2, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NonStackableGutsPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<IndomitablePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(IndomitablePower).Name].BaseValue, Owner.Creature, this);
    }
}