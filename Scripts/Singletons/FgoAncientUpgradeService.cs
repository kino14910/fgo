using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Fgo.Scripts.Singletons;

/// <summary>
///     先古之民（Ancient）房间的玛修衍生物卡进化服务。
///     注册为 Run 级单例，接收 AfterRoomEntered（局内钩子）。
/// </summary>
[RegisterSingleton]
public class FgoAncientUpgradeService() : HookedSingletonModel(HookType.Run)
{
    /// <summary>
    ///     进入先古之民（Ancient 事件）房间时，将 FGO 玩家的玛修衍生物卡按进化链转换升级一级。
    /// </summary>
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is EventRoom { CanonicalEvent: AncientEventModel })
        {
            var player = CurrentRunState?.Players.FirstOrDefault(p => p.Character is FgoCharacter);
            if (player == null) return;

            // 刚进入游戏（第一层）时也会先进入先古之民房间，此时牌组/卡堆尚未就绪，
            // 且玩家尚未经历任何战斗，不应触发进化；从 TotalFloor >= 2 开始才升级。
            if (player.RunState.TotalFloor <= 1) return;

            await FgoCardActions.TryUpgradeDerivativeMash(player);
        }
    }
}