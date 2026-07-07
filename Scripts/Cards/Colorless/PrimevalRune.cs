using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.Colorless;

public class PrimevalRune : FgoColorlessCard
{
    public PrimevalRune() : base(1, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
    {
        WithPower<WeakPower>(2);
        WithPower<VulnerablePower>(2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[typeof(WeakPower).Name].BaseValue,
                Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars[typeof(VulnerablePower).Name].BaseValue, Owner.Creature, this);
        }
    }
}