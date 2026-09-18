using Fgo.Scripts.Fields;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class GreatRamNautilus() : NobleCardModel(2, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.Waterside.ToHoverTip(),
        FgoFieldId.ImaginarySpace.ToHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(40)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(12m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var hasWaterside = FgoField.Has(Owner.Creature.CombatState, FgoFieldId.Waterside);
        var hasImaginarySpace = FgoField.Has(Owner.Creature.CombatState, FgoFieldId.ImaginarySpace);
        var baseDmg = (int)DynamicVars.Damage.BaseValue;
        var totalDamage = hasWaterside || hasImaginarySpace ? (int)(baseDmg * 1.5m) : baseDmg;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        if (hasWaterside) await PlayerCmd.GainEnergy(1, Owner);
    }
}