using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class KnightStance : FgoCardModel
{
    public KnightStance() : base(1, CardType.Skill,
        CardRarity.Common, TargetType.Self)
    {
        WithBlock(11, 3);
        WithVar("DamageReduction", 25);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["DamageReduction"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}