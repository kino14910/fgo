using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class ImitationImmortality() : FgoCardModel(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NonStackableGutsPower>(5),
        new PowerVar<PlatingPower>(6),
        new PowerVar<NpPerTurnPower>(10),
        ModCardVars.Int("DamageReduction", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NonStackableGutsPower)].UpgradeValueBy(3);
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(3);
        DynamicVars[nameof(NpPerTurnPower)].UpgradeValueBy(5);
        DynamicVars["DamageReduction"].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NonStackableGutsPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpPerTurnPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["DamageReduction"].BaseValue, Owner.Creature, this);
    }
}