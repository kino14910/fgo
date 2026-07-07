using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     拟似展开／人理之础：马修初始宝具卡。
/// </summary>
public class Camelot : NobleCardModel
{
    public Camelot() : base(1, CardType.Power, TargetType.Self)
    {
        WithPower<PlatingPower>(1, 1);
        WithVar("DamageReduction", 20, 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(PlatingPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["DamageReduction"].BaseValue, Owner.Creature, this);
    }
}