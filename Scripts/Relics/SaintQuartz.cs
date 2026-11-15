using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

/// <summary>
///     圣晶石: FGO 角色的初始遗物。每层 +1 计数，≥3 时右键消耗 3 计数，
///     从尚未拥有的宝具中随机 3 张三选一，选中的卡加入 NobleDeck。
///     被点金石精炼后升级为呼符（SummonTicket）；计数存于按玩家的 FgoRunState，替换实例后保留。
/// </summary>
[RegisterCharacterStarterRelic(typeof(FgoCharacter))]
public class SaintQuartz : FgoRelic, IModRightClickableRelic
{
    public const int CostPerChoice = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;
    public override int DisplayAmount => QuartzCounter;

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        if (!FgoQuartzSummon.CanOpenSelection()) return false;
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner)) return false;
        return QuartzCounter >= CostPerChoice;
    }

    /// <summary>
    ///     右键托管动作在各端重放，CardSelectCmd 自带本地弹界面 / 远端等待的选择同步；
    ///     GameActionPlayerChoiceContext 会挂起当前动作让出队列，不会堵住其它玩家。
    ///     计数守卫放在结算里，避免各端计数短暂滞后时只有部分端进入选择协议。
    /// </summary>
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (context.PlayerChoiceContext is not { } choiceContext) return;

        var player = context.Player;
        var options = FgoQuartzSummon.BuildOptions(player, this, 3);
        if (options.Count == 0)
        {
            Flash();
            return;
        }

        var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player, canSkip: true);
        if (selected == null)
        {
            Flash();
            return;
        }

        if (!await FgoQuartzSummon.Settle(player, selected))
        {
            Flash();
            return;
        }

        Flash();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        // 读档重建当前房间时（仅客户端）重放本动作：已在存档里计过，跳过以免相对主机多 +1。
        if (FgoQuartzSync.ShouldSkipRoomEntry()) return Task.CompletedTask;
        IncrementQuartzOnRoomEntry(CostPerChoice);
        return Task.CompletedTask;
    }
}
