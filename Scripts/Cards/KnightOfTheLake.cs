using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class KnightOfTheLake : FgoCardModel
{
    public KnightOfTheLake() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("CritDamage", 30, 20);
        WithVar("Stars", 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.Creature.HasPower<CriticalDamagePower>())
            await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
                DynamicVars["CritDamage"].BaseValue, Owner.Creature, this);
        else
            await FgoStarCmd.AddStars(DynamicVars["Stars"].IntValue);
    }
}