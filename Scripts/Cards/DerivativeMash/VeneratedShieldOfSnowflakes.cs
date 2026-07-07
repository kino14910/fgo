using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class VeneratedShieldOfSnowflakes : FgoCardModel
{
    public VeneratedShieldOfSnowflakes() : base(1, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        WithVar("DamageReduction", 20);
        WithNp(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, self, DynamicVars["DamageReduction"].BaseValue,
            Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature, 30m, Owner.Creature, this);
    }
}