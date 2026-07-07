using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class Revelation : FgoCardModel
{
    public Revelation() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithPower<VulnerablePower>(1, 1);
        WithPower<WeakPower>(1, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target!,
            DynamicVars[typeof(VulnerablePower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target!, DynamicVars[typeof(WeakPower).Name].BaseValue,
            Owner.Creature, this);
    }
}