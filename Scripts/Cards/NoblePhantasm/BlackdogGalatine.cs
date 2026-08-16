using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BlackdogGalatine() : NobleCardModel(1, CardType.Attack, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<MaxHpPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(16),
        ModCardVars.Power<MaxHpPower>(6),
        ModCardVars.Int("Energy", 2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_fire_burning")
            .Execute(choiceContext);
        // 获得再生
        await PowerCmd.Apply<MaxHpPower>(choiceContext, Owner.Creature, DynamicVars[nameof(MaxHpPower)].BaseValue,
            Owner.Creature, this);
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }
}