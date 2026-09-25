using Fgo.Scripts.Character;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Powers;

/// <summary>
///     恋歌: 本回合内所有友方攻击产生的暴击星，统一由能力持有者获得。
///     由 <c>FgoBattleHooks.AfterDamageGiven</c> 在每次攻击命中结算暴击星时查询本能力。
/// </summary>
public class SongOfLovePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     找出本场战斗中正在收星的玩家（能力持有者）。
    ///     多人模式下每端各持一份能力实例，这里按 <see cref="ICombatState.Players" /> 的确定顺序取首个，
    ///     保证各端解析结果一致（并存多张时以最先打出的为准）。
    /// </summary>
    public static Player? ResolveStarReceiver(ICombatState? combatState)
    {
        if (combatState == null) return null;

        foreach (var player in combatState.Players)
            if (player.Character is FgoCharacter && player.Creature.HasPower<SongOfLovePower>())
                return player;

        return null;
    }

    /// <summary>本回合结束（我方回合结束阶段）即失效。</summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        Flash();
        await PowerCmd.Remove(this);
    }
}
