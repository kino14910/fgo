using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class GraceUnexpectedBirth : FgoCardModel
{
    public GraceUnexpectedBirth() : base(0, CardType.Skill,
        CardRarity.Common, TargetType.Self)
    {
        WithPower<SealNpPower>(1);
        WithNp(30);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
        await PowerCmd.Apply<SealNpPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(SealNpPower).Name].BaseValue, Owner.Creature, this);
    }
}