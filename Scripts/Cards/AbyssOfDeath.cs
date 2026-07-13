using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards;

public class AbyssOfDeath() : FgoCardModel(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<RegenPower>(3),
        new PowerVar<DeathOfDeathPower>(5)
    ];
    
    protected override void OnUpgrade()
    {
        DynamicVars[nameof(RegenPower)].UpgradeValueBy(2);
        DynamicVars[nameof(DeathOfDeathPower)].UpgradeValueBy(3);
    }
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[nameof(RegenPower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<DeathOfDeathPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(DeathOfDeathPower)].BaseValue, Owner.Creature, this);
    }
}