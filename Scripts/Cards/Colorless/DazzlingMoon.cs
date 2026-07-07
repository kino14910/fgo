using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.Colorless;

public class DazzlingMoon : FgoColorlessCard
{
    public DazzlingMoon() : base(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithPower<StrengthPower>(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<DazzlingMoonPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        WithKeywords(CardKeyword.Innate);
    }
}