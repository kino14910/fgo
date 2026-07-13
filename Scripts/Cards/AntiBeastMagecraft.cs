using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AntiBeastMagecraft() : FgoCardModel(1, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AntiBeastMagecraftPower>(3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(AntiBeastMagecraftPower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<AntiBeastMagecraftPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(AntiBeastMagecraftPower)].BaseValue, Owner.Creature, this);
    }
}