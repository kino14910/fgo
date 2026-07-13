using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class ItsInevitable() : FgoCardModel(1, CardType.Attack,
        CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        ModCardVars.Int("Boost", 4)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Boost"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_fire")
            .Execute(choiceContext);
        await PowerCmd.Apply<ItsInevitablePower>(choiceContext, Owner.Creature, DynamicVars.Damage.IntValue,
            Owner.Creature, this);
        var power = Owner.Creature.GetPower<ItsInevitablePower>();
        if (power != null)
            power.Boost = DynamicVars["Boost"].IntValue;
    }
}