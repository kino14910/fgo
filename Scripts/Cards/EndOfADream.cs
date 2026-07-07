using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class EndOfADream : FgoCardModel
{
    public EndOfADream() : base(1, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithVar("ExhaustCount", 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PermanentSleepPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}