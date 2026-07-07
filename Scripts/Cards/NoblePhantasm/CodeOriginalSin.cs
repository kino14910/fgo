using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class CodeOriginalSin : NobleCardModel
{
    public CodeOriginalSin() : base(1, CardType.Attack, TargetType.Self)
    {
        WithDamage(40, 8);
        WithNp(20);
        WithPower<DoomPower>(8);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, CombatState!.HittableEnemies,
            DynamicVars.Damage.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            Owner.Creature);
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars[typeof(DoomPower).Name].BaseValue,
                Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}