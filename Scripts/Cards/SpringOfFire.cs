using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class SpringOfFire : FgoCardModel
{
    public SpringOfFire() : base(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithHeal(20, 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature, DynamicVars.Heal.BaseValue,
            Owner.Creature, this);
    }
}