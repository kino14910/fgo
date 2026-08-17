using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class FetchFailnaught() : NobleCardModel(1, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CursePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 计算伤害：基础伤害 * (1 + 诅咒层数 * 0.1)
        ModCardVars.ComputedDamage("FetchFailnaughtDamage",
            30,
            (card, target) =>
            {
                var baseDmg = card?.DynamicVars["FetchFailnaughtDamage"].BaseValue ?? 30;
                var curseAmt = target?.GetPowerAmount<CursePower>() ?? 0;
                return baseDmg * (1m + curseAmt * 0.1m);
            }),
        ModCardVars.Int("CurseMultiplier", 1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["FetchFailnaughtDamage"].UpgradeValueBy(8);
        DynamicVars["CurseMultiplier"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var target = play.Target;
        if (target == null) return;

        var finalDamage = DynamicVars.EvaluateValueOrDefault("FetchFailnaughtDamage");

        await DamageCmd.Attack(finalDamage)
            .FromCard(this, play)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await PowerCmd.Apply<CursePower>(choiceContext, target, 3m, Owner.Creature, this);

        // 若目标已有诅咒，再根据当前层数 × 倍率 施加额外诅咒
        if (target.HasPower<CursePower>())
        {
            var curAmt = target.GetPowerAmount<CursePower>();
            var multiplier = DynamicVars.EvaluateValueOrDefault("CurseMultiplier");
            await PowerCmd.Apply<CursePower>(choiceContext, target, curAmt * multiplier,
                Owner.Creature, this);
        }
    }
}