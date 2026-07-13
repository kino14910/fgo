using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class KurKigalIrkalla(): NobleCardModel(1, CardType.Attack, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(26, ValueProp.Move)
    ];

    protected override bool ShouldGlowGoldInternal =>
        Owner.Creature.HasPower<BlessingOfKurPower>();

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
            await CreatureCmd.Damage(choiceContext, enemy,
                (int)(enemy.MaxHp / 10.0f), ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                Owner.Creature);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        var blessing = Owner.Creature.GetPower<BlessingOfKurPower>();
        if (blessing != null)
        {
            blessing.TriggerFlash();
            await CreatureCmd.Heal(Owner.Creature, blessing.Amount, false);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, blessing.Amount / 3m, Owner.Creature,
                this);
            await PowerCmd.Remove(blessing);
        }
    }
}