using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class WitchOfSalem() : FgoCardModel(2, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(3),
        new PowerVar<WeakPower>(3),
        ModCardVars.Int("TerrorChance", 30),
        ModCardVars.Int("VsTerrorDamage", 50),
        ModCardVars.Int("Np", 20)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[nameof(WeakPower)].BaseValue,
                Owner.Creature, this);
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars["TerrorChance"].BaseValue, Owner.Creature,
                this);
        }

        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["VsTerrorDamage"].BaseValue, Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue);
    }
}