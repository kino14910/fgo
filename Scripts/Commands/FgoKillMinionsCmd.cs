using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.ManagedActions;

namespace Fgo.Scripts.Commands;

public static class FgoKillMinionsCmd
{
    /// <summary>
    ///     托管网络动作: "即死全部爪牙并按只数获得 NP" 的联机同步（详见 FgoNoblePhantasmCmd.SyncDescriptor 注释）。
    ///     筛选爪牙、击杀、加 NP 全部放进这个动作的执行体，在所有 peer 的动作队列同一确定位置各跑一遍：
    ///     三者读到的 CombatState 是同刻状态，NP 变更也落在同一 GameAction 内，避免各端本地各自判定导致的分歧。
    ///     必须在 Entry.Init 注册，保证任何 peer 发起前本端已注册（ExecuteAction 按 opcode 查找）。
    /// </summary>
    internal static readonly RitsuLibManagedNetActionDescriptor<int> SyncDescriptor = new(
        Entry.ModId,
        "kill_minions",
        static np => BitConverter.GetBytes(np),
        static bytes => BitConverter.ToInt32(bytes),
        ExecuteManaged,
        GameActionType.CombatPlayPhaseOnly);

    private static async Task ExecuteManaged(RitsuLibManagedNetActionContext<int> context)
    {
        var combatState = context.Player.Creature.CombatState;
        if (combatState == null) return;

        var minions = combatState.HittableEnemies
            .Where(static enemy => enemy.HasPower<MinionPower>())
            .ToList();

        foreach (var minion in minions)
        {
            await CreatureCmd.Kill(minion);
            await FgoResCmd.ModifyNp(context.Message, context.Player);
        }
    }

    /// <summary>
    ///     出牌入口: 仅由卡牌 OnPlay 在本机调用（Request 会校验 owner == 本机 NetId，其它端自动被拒）。
    /// </summary>
    public static bool Request(Player player, int npPerMinion)
    {
        return RitsuLibManagedNetActions.Request(RunManager.Instance, SyncDescriptor, npPerMinion, player.NetId);
    }
}