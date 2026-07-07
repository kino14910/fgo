using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards;

public class Coaching : FgoCardModel
{
    public Coaching() : base(0, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithEnergy(2, 1);
        WithDamage(2);
        WithHeal(2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CreatureCmd.Damage(choiceContext, self, DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, self);
        await CreatureCmd.GainMaxHp(self, DynamicVars.Heal.BaseValue);
    }
}