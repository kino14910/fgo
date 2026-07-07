using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class PursuerofLove : FgoCardModel
{
    public PursuerofLove() : base(1, CardType.Skill,
        CardRarity.Common, TargetType.AnyEnemy)
    {
        WithPower<StrengthPower>(1);
        WithVar("Pursue", 2, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target!,
            DynamicVars[typeof(StrengthPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target!, DynamicVars["Pursue"].BaseValue, Owner.Creature,
            this);
    }
}