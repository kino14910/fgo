using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class LakeTexcoco : FgoCardModel
{
    public LakeTexcoco() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithNp(20, 10);
        WithPower<NpPerTurnPower>(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
        await PowerCmd.Apply<WatersidePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpPerTurnPower).Name].BaseValue, Owner.Creature, this);
    }
}