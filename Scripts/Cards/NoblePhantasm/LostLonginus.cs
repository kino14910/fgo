using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class LostLonginus(): NobleCardModel(2, CardType.Attack, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(24, ValueProp.Move),
        new PowerVar<RegenPower>(6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
        DynamicVars[nameof(RegenPower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // 无敌贯通
        await PowerCmd.Apply<IgnoresInvincibilityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        // 获得再生
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[nameof(RegenPower)].BaseValue,
            Owner.Creature, this);
    }
}