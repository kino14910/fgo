using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class LostLonginus : NobleCardModel
{
    public LostLonginus() : base(2, CardType.Attack, TargetType.Self)
    {
        WithDamage(24, 6);
        WithPower<RegenPower>(6, 3);
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
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[typeof(RegenPower).Name].BaseValue,
            Owner.Creature, this);
    }
}