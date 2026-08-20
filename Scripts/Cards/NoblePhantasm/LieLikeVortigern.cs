using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class LieLikeVortigern() : NobleCardModel(3, CardType.Attack, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<IntangiblePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(25),
        ModCardVars.Power<IntangiblePower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(7m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await PowerCmd.Apply<StrengthPower>(choiceContext, CombatState!.HittableEnemies, -2m, Owner.Creature, this);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(IntangiblePower)].BaseValue, Owner.Creature, this);

        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(IntangiblePower)].BaseValue, Owner.Creature, this);
    }
}