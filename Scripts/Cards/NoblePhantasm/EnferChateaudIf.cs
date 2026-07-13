using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class EnferChateaudIf(): NobleCardModel(1, CardType.Power, TargetType.Self) {

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.Relics.Any(r => r is WingedBoots))
            await RelicCmd.Obtain(ModelDb.Relic<WingedBoots>().ToMutable(), Owner, Owner.Relics.Count);
    }
}