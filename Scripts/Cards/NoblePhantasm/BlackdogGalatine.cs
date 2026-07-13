using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BlackdogGalatine(): NobleCardModel(1, CardType.Attack, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16, ValueProp.Move),
        new PowerVar<RegenPower>(6),
        ModCardVars.Int("Energy", 2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars[nameof(RegenPower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_fire")
            .Execute(choiceContext);
        // 获得再生
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[nameof(RegenPower)].BaseValue,
            Owner.Creature, this);
        // 获得 [E][E]
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }
}