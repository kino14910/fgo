using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class FacelessMoonPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        var hand = PileType.Hand.GetPile(player).Cards.Count;
        await CreatureCmd.GainBlock(Owner, hand, ValueProp.Unpowered, null);
        await FgoResCmd.ModifyStars(hand, player);
        await PowerCmd.Remove(this);
    }
}