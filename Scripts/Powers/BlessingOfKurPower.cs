using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Powers;

public class BlessingOfKurPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card is KurKigalIrkalla)
        {
            Flash();
            if (Owner.Player is null) return;
            await CreatureCmd.Heal(Owner.Player.Creature, Amount, false);
            await PowerCmd.Apply<StrengthPower>(context, Owner.Player.Creature, Amount / 3m, Owner.Player.Creature,
                cardPlay.Card);
            await PowerCmd.Remove(this);
        }
    }
}