using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

public class Parfait : FgoDishCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Heal(20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(20);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        foreach (var ally in Allies)
            await CreatureCmd.Heal(ally, DynamicVars.Heal.BaseValue);
    }
}