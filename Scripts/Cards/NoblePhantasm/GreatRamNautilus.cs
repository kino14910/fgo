using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class GreatRamNautilus : NobleCardModel
{
    public GreatRamNautilus() : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
        WithDamage(40, 12);
        WithEnergy(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var hasWaterside = Owner.Creature.HasPower<WatersidePower>();
        var hasImaginarySpace = Owner.Creature.HasPower<ImaginarySpacePower>();
        var baseDmg = (int)DynamicVars.Damage.BaseValue;
        var totalDamage = hasWaterside || hasImaginarySpace ? (int)(baseDmg * 1.5m) : baseDmg;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        if (hasWaterside) await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(12m);
    }
}