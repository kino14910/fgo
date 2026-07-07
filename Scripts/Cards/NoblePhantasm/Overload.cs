using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Overload : NobleCardModel
{
    public Overload() : base(2, CardType.Attack, TargetType.AnyEnemy)
    {
        WithDamage(10, 4);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        // 宝具值获取提升
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        // 去除敌人的格挡值
        if (play.Target!.Block > 0) await CreatureCmd.LoseBlock(play.Target!, play.Target.Block);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}