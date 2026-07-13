using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class WorldsEndFlowerGarden() : FgoCardModel(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("NPOnCrit", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["NPOnCrit"].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<WorldsEndFlowerGardenPower>(choiceContext, Owner.Creature,
            DynamicVars["NPOnCrit"].BaseValue, Owner.Creature, this);
    }
}