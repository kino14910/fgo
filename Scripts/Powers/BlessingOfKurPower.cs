using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Powers;

public class BlessingOfKurPower : FgoPowerModel
{
    public const int MaxHpPerStack = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner) return;
        if (Owner.Player is null) return;

        if (cardPlay.Card is KurKigalIrkalla)
        {
            Flash();
            await PowerCmd.Apply<MaxHpPower>(context, Owner, Amount * MaxHpPerStack, Owner, null);
            await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}