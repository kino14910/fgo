using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class FromTheWorldsEnd : FgoCardModel
{
    public FromTheWorldsEnd() : base(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<WeakPower>(3);
        WithVar("Turns", 3, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[typeof(WeakPower).Name].BaseValue,
                Owner.Creature, this);
        await PowerCmd.Apply<FromTheWorldsEndPower>(choiceContext, Owner.Creature, DynamicVars["Turns"].BaseValue,
            Owner.Creature, this);
    }
}