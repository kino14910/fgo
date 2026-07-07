using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class HeroicKingPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     暴击联动：每次暴击触发时，为玩家赋予 1 层王威。
    /// </summary>
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (Owner != dealer) return;
        if (!CriticalDamagePower.CritTriggered) return;
        if (Amount <= 0) return;

        Flash();
        await PowerCmd.Apply<HeroicKingPower>(choiceContext, dealer, 1, dealer, cardSource);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.TotalDamage <= 0) return;

        var player = Owner.Player;
        if (player?.PlayerCombatState?.DiscardPile?.Cards == null) return;

        var card = player.PlayerCombatState.DiscardPile.Cards
            .FirstOrDefault(card => card is HeroicKing);
        if (card == null) return;

        Flash();
        await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, this, true);
        await PowerCmd.Decrement(this);
    }
}