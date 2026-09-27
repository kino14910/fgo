using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

/// <summary>
///     〔纳刀〕状态: 直到〔拔刀·神威〕打出前，拥有者不能打出其他攻击牌。
///     <para>
///         出牌封锁走官方 <see cref="MegaCrit.Sts2.Core.Models.AbstractModel.ShouldPlay" /> 钩子
///         （原版 SlothPower / RingingPower 同路），因此卡面会正常显示为不可打出、
///         <c>PlayerCombatState.HasCardsToPlay()</c> 也会同步。
///         其他玩家的牌不受影响（多人各自只锁自己的攻击牌）。
///     </para>
///     <para>
///         「只有打出拔刀·神威才解除」是作者确认过的严格语义：若下次选宝具时选了别的宝具，
///         本场战斗就再拿不到拔刀·神威，攻击牌将被锁死到战斗结束（不拦技能/能力牌，
///         仍可靠打防御牌攒宝具值重开一轮，故不会卡死流程）。若要放行其他宝具攻击牌，
///         在 <see cref="ShouldPlay" /> 里对 NobleCardModel 提前 return true，并把解除条件
///         一并放宽即可。
///     </para>
/// </summary>
public class SwordSheathingPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner.Creature != Owner) return true;
        if (card is BattouKamui) return true;
        return card.Type != CardType.Attack;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not BattouKamui) return;
        if (cardPlay.Player.Creature != Owner) return;

        Flash();
        await PowerCmd.Remove(this);
    }
}
