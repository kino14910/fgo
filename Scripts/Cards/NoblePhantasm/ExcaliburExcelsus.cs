using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class ExcaliburExcelsus : NobleCardModel
{
    public ExcaliburExcelsus() : base(3, CardType.Attack, TargetType.Self)
    {
        WithDamage(16, 6);
        WithBlock(6);
        WithVar("Strength", 1);
        WithNp(10);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // 无敌贯通
        await PowerCmd.Apply<IgnoresInvincibilityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        var enemyCount = CombatState!.HittableEnemies.Count();
        // 每有一名敌人，获得格挡、力量、宝具值
        if (enemyCount > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["Block"].BaseValue * enemyCount,
                ValueProp.Unpowered, play);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
                DynamicVars["Strength"].BaseValue * enemyCount, Owner.Creature, this);
            await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue * enemyCount);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}