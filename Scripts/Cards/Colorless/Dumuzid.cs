using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless;

[RegisterCard(typeof(CurseCardPool))]
public class Dumuzid() : FgoBaseCardModel(3, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DeckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(DeckVersion);
            DeckVersion = null;
        }
    }
}