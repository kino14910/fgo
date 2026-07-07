using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class ImitationImmortality : FgoCardModel
{
    public ImitationImmortality() : base(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<NonStackableGutsPower>(5, 3);
        WithPower<PlatingPower>(6, 3);
        WithPower<NpPerTurnPower>(10, 5);
        WithVar("DamageReduction", 10, 5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NonStackableGutsPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(PlatingPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpPerTurnPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["DamageReduction"].BaseValue, Owner.Creature, this);
    }
}