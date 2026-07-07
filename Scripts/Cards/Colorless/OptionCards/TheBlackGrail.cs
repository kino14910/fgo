using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless.OptionCards;

public class TheBlackGrail : FgoColorlessCard
{
    public TheBlackGrail() : base(0, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        WithPower<NpDamagePower>(30, 20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpDamagePower).Name].BaseValue, Owner.Creature, this);
    }
}