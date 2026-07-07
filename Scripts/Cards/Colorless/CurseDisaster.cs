using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless;

public class CurseDisaster : FgoColorlessCard
{
    public CurseDisaster() : base(-2, CardType.Status,
        CardRarity.Status, TargetType.Self)
    {
        WithKeywords(CardKeyword.Ethereal);
        WithPower<CursePower>(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<CursePower>(choiceContext, Owner.Creature, DynamicVars[typeof(CursePower).Name].BaseValue,
            Owner.Creature, this);
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<CursePower>(choiceContext, enemy, DynamicVars[typeof(CursePower).Name].BaseValue,
                Owner.Creature, this);
    }
}