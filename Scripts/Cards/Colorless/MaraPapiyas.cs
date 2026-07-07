using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless;

public class MaraPapiyas : FgoColorlessCard
{
    public MaraPapiyas() : base(0, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithHeal(6, 2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, 2m, true);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);
    }
}