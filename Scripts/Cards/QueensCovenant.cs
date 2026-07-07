using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class QueensCovenant : FgoCardModel
{
    public QueensCovenant() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<StrengthPower>(2, 1);
        WithPower<DexterityPower>(2, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var strAmount = DynamicVars.Strength.BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strAmount, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(DexterityPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<MyFairSoldierPower>(choiceContext, Owner.Creature, strAmount, Owner.Creature, this);
    }
}