using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class WitchOfSalem : FgoCardModel
{
    public WitchOfSalem() : base(2, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
    {
        WithTags(FgoTags.Foreigner);
        WithPower<VulnerablePower>(3);
        WithPower<WeakPower>(3);
        WithVar("TerrorChance", 30);
        WithVar("VsTerrorDamage", 50);
        WithNp(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars[typeof(VulnerablePower).Name].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[typeof(WeakPower).Name].BaseValue,
                Owner.Creature, this);
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars["TerrorChance"].BaseValue, Owner.Creature,
                this);
        }

        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["VsTerrorDamage"].BaseValue, Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}