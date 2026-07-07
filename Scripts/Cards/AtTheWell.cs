using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class AtTheWell : FgoCardModel
{
    public AtTheWell() : base(0, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithPower<RegenPower>(6, 6);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        var debuffs = self.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
        foreach (var debuff in debuffs) await PowerCmd.Remove(debuff);
        await PowerCmd.Apply<AtTheWellPower>(choiceContext, self, DynamicVars["RegenPower"].BaseValue, Owner.Creature,
            this);
    }
}