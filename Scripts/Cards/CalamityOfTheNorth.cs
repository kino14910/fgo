using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class CalamityOfTheNorth : FgoCardModel
{
    public CalamityOfTheNorth() : base(2, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithKeywords(CardKeyword.Retain);
        WithPower<PoisonPower>(5, 3);
        WithPower<CursePower>(5, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, DynamicVars[typeof(PoisonPower).Name].BaseValue,
                Owner.Creature, this);
            await PowerCmd.Apply<CursePower>(choiceContext, enemy, DynamicVars[typeof(CursePower).Name].BaseValue,
                Owner.Creature, this);
        }
    }
}