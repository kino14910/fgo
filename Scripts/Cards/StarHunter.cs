using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class StarHunter : FgoCardModel
{
    public StarHunter() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithStar(8);
        WithPower<CriticalDamageOncePower>(50, 50);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await FgoStarCmd.AddStars(DynamicVars["Star"].IntValue);
        await PowerCmd.Apply<CriticalDamageOncePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(CriticalDamageOncePower).Name].BaseValue, Owner.Creature, this);
    }
}