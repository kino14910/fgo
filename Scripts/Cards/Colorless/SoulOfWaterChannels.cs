using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless;

public class SoulOfWaterChannels : FgoCardModel
{
    public SoulOfWaterChannels() : base(0, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust, CardKeyword.Retain);
        WithTags(FgoTags.Foreigner);
        WithVar("Stars", 10, 5);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<CursePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await FgoStarCmd.AddStars(DynamicVars["Stars"].IntValue);
    }
}