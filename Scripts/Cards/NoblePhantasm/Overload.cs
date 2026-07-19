using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Overload(): NobleCardModel(2, CardType.Attack, TargetType.AnyEnemy) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        // 宝具值获取提升
        await PowerCmd.Apply<NpRatePower>(choiceContext, self, 1m, self, this);

        // 去除敌人的格挡值
        if (play.Target!.Block > 0) await CreatureCmd.LoseBlock(choiceContext, play.Target!, play.Target.Block, self);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}