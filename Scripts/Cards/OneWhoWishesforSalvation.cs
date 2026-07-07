using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class OneWhoWishesforSalvation : FgoCardModel
{
    public OneWhoWishesforSalvation() : base(1, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<NpPerTurnPower>(10, 5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpPerTurnPower).Name].BaseValue, Owner.Creature, this);
    }
}