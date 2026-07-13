using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class FromTheWorldsEnd() : FgoCardModel(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(3),
        ModCardVars.Int("Turns", 3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Turns"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[nameof(WeakPower)].BaseValue,
                Owner.Creature, this);
        await PowerCmd.Apply<FromTheWorldsEndPower>(choiceContext, Owner.Creature, DynamicVars["Turns"].BaseValue,
            Owner.Creature, this);
    }
}