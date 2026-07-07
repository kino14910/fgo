using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class LuckySpinning : FgoCardModel
{
    public LuckySpinning() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithStar(1, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<LuckySpinningPower>(choiceContext, Owner.Creature, DynamicVars["Star"].BaseValue,
            Owner.Creature, this);
    }
}