using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class CodeOriginalSin(): NobleCardModel(1, CardType.Attack, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(40, ValueProp.Move),
        ModCardVars.Int("Np", 20),
        new PowerVar<DoomPower>(8)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, CombatState!.HittableEnemies,
            DynamicVars.Damage.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            Owner.Creature);
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars[nameof(DoomPower)].BaseValue,
                Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue);
    }
}