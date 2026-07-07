using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Desterrennacht : NobleCardModel
{
    public Desterrennacht() : base(2, CardType.Power, TargetType.Self)
    {
        WithTags(FgoTags.Foreigner);
        WithPower<StrengthPower>(2, 1);
        WithVar("CritDamage", 60, 10);
        WithVar("TerrorChance", 60);
        WithVar("StarRegen", 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 给予所有敌人3层恐怖(60%概率)
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars["TerrorChance"].BaseValue, Owner.Creature,
                this);

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(StrengthPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature, DynamicVars["CritDamage"].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<StarsPerTurnPower>(choiceContext, Owner.Creature, DynamicVars["StarRegen"].BaseValue,
            Owner.Creature, this);
    }
}