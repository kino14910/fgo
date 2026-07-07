using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class IraLupus : NobleCardModel
{
    public IraLupus() : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
        WithVar(new DamageVar(24m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move));
        WithPower<VulnerablePower>(2, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, play.Target!, DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target!,
            DynamicVars[typeof(VulnerablePower).Name].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}