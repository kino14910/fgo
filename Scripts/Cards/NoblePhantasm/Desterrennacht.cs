using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Desterrennacht() : NobleCardModel(2, CardType.Power, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(2),
        ModCardVars.Int("CritDamage", 60),
        ModCardVars.Int("TerrorChance", 60),
        ModCardVars.Int("StarRegen", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
        DynamicVars["CritDamage"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 给予所有敌人3层恐怖(60%概率)
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars["TerrorChance"].BaseValue, Owner.Creature,
                this);

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature, DynamicVars["CritDamage"].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<StarsPerTurnPower>(choiceContext, Owner.Creature, DynamicVars["StarRegen"].BaseValue,
            Owner.Creature, this);
    }
}