using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class VeneratedShieldOfSnowflakes() : FgoCardModel(1, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            ModCardVars.Int("DamageReduction", 20),
            ModCardVars.Int("Np", 20)
        ];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, self, DynamicVars["DamageReduction"].BaseValue,
            Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature, 30m, Owner.Creature, this);
    }
}