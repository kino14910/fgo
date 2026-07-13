using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class IraLupus(): NobleCardModel(1, CardType.Attack, TargetType.AnyEnemy) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(24m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move),
        new PowerVar<VulnerablePower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars[nameof(VulnerablePower)].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, play.Target!, DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target!,
            DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
    }
}