using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class FetchFailnaught : NobleCardModel
{
    public FetchFailnaught() : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
        WithDamage(30, 8);
        WithVar("CurseMultiplier", 1, 1);
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