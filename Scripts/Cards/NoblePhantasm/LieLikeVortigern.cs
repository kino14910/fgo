using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class LieLikeVortigern(): NobleCardModel(3, CardType.Attack, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(25, ValueProp.Move),
        new PowerVar<IntangiblePower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(7m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -2m, Owner.Creature, this);
            await PowerCmd.Apply<IntangiblePower>(choiceContext, enemy,
                DynamicVars[nameof(IntangiblePower)].BaseValue, Owner.Creature, this);
        }

        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(IntangiblePower)].BaseValue, Owner.Creature, this);
    }
}