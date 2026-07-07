using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class ObscurantWallofChalk : NobleCardModel
{
    public ObscurantWallofChalk() : base(0, CardType.Skill, TargetType.Self)
    {
        WithNp(30, 20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<AntiPurgeDefensePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}