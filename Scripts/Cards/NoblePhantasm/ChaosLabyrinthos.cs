using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class ChaosLabyrinthos : NobleCardModel
{
    public ChaosLabyrinthos() : base(1, CardType.Skill, TargetType.Self)
    {
        WithPower<StrengthPower>(3, 2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -DynamicVars.Strength.BaseValue, Owner.Creature,
                this);
    }
}