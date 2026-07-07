using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class BlessingOfKur : FgoCardModel
{
    public BlessingOfKur() : base(1, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("TempHP", 6, 3);
        WithPower<StrengthPower>(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<BlessingOfKurPower>(choiceContext, Owner.Creature, DynamicVars["TempHP"].BaseValue,
            Owner.Creature, this);
    }
}