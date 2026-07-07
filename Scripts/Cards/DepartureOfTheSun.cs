using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class DepartureOfTheSun : FgoCardModel
{
    public DepartureOfTheSun() : base(1, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithTags(FgoTags.Foreigner);
        WithVar("StarThreshold", 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<DepartureOfTheSunPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        var power = Owner.Creature.GetPower<DepartureOfTheSunPower>();
        if (power != null)
            power.StarThreshold = DynamicVars["StarThreshold"].IntValue;
    }
}