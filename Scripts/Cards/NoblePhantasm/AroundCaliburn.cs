using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class AroundCaliburn(): NobleCardModel(2, CardType.Power, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(4),
        new PowerVar<AntiPurgeDefensePower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(AntiPurgeDefensePower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<AntiPurgeDefensePower>(choiceContext, self, DynamicVars[nameof(AntiPurgeDefensePower)].BaseValue,
            Owner.Creature, this);
    }
}