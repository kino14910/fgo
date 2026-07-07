using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class WisdomOfThePeople : FgoCardModel
{
    public WisdomOfThePeople() : base(3, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithHeal(20);
        WithNp(30);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);

        var self = Owner.Creature;
        var debuffs = self.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
        if (debuffs.Count > 0)
        {
            var random = debuffs[Random.Shared.Next(debuffs.Count)];
            await PowerCmd.Remove(random);
        }

        if (IsUpgraded) await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}