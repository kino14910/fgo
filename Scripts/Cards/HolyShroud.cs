using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class HolyShroud : FgoCardModel
{
    public HolyShroud() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<ReducePercentDamagePower>(20);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(ReducePercentDamagePower).Name].BaseValue, Owner.Creature, this);
    }
}