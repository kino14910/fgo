using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class ExcaliburGalatine() : NobleCardModel(2, CardType.Attack, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VigorPower>(),
        FgoFieldId.Sunlight.ToHoverTip(),
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(24),
        ModCardVars.Power<VigorPower>(4),
        ModCardVars.Power<CriticalDamagePower>(50),
        ModCardVars.Int("SunlightTurns", 3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        FgoField.Add(Owner.Creature.CombatState, FgoFieldId.Sunlight,
            (int)DynamicVars["SunlightTurns"].BaseValue);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, DynamicVars[nameof(VigorPower)].BaseValue,
            Owner.Creature, this);
        if (FgoField.Has(Owner.Creature.CombatState, FgoFieldId.Sunlight))
            await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
                DynamicVars[nameof(CriticalDamagePower)].BaseValue, Owner.Creature, this);
    }
}