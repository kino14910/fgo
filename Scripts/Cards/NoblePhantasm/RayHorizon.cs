using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class RayHorizon : NobleCardModel
{
    public RayHorizon() : base(0, CardType.Skill, TargetType.Self)
    {
        WithNp(50, 50);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        FgoCardActions.ForceNextNpCard(Owner);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue, choiceContext, Owner);
        await PowerCmd.Apply<InvincibilityTurnPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}