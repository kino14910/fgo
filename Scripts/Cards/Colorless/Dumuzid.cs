using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless;

[RegisterCard(typeof(CurseCardPool), Inherit = true)]
public class Dumuzid() : FgoBaseCardModel(3, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.RemoveFromCombat(this);
        await CardPileCmd.RemoveFromDeck(this);
    }
}