using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class FetchFailnaught(): NobleCardModel(1, CardType.Attack, TargetType.AnyEnemy) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(30, ValueProp.Move),
        ModCardVars.Int("CurseMultiplier", 1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
        DynamicVars["CurseMultiplier"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var target = play.Target!;
        // 伤害提高：敌人每有一层诅呪+10%
        var bonus = 1m;
        if (target.HasPower<CursePower>())
            bonus += target.GetPowerAmount<CursePower>() * 0.1m;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue * bonus)
            .FromCard(this, play)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await PowerCmd.Apply<CursePower>(choiceContext, target, 3m, Owner.Creature, this);
        if (target.HasPower<CursePower>())
        {
            var curAmt = target.GetPowerAmount<CursePower>();
            await PowerCmd.Apply<CursePower>(choiceContext, target, curAmt * DynamicVars["CurseMultiplier"].IntValue,
                Owner.Creature, this);
        }
    }
}