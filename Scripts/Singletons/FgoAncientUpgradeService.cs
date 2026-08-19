using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Fgo.Scripts.Singletons;

/// <summary>
///     先古之民（Ancient）房间的玛修衍生物卡进化服务。
///     注册为 Run 级单例，接收 AfterRoomEntered（局内钩子），与遗物是否被获得无关，
///     因此玩家未获得圣晶石/召唤券时进入 Ancient 房间也能触发卡牌进化。
/// </summary>
[RegisterSingleton]
public class FgoAncientUpgradeService() : HookedSingletonModel(HookType.Run)
{
    /// <summary>
    ///     进入先古之民（Ancient 事件）房间时，将 FGO 玩家的玛修衍生物卡按进化链转换升级一级。
    /// </summary>
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is EventRoom { CanonicalEvent: AncientEventModel and not Neow })
        {
            var player = CurrentRunState?.Players.FirstOrDefault(p => p.Character is FgoCharacter);
            if (player != null)
                await FgoCardActions.TryUpgradeDerivativeMash(player);
        }

        await Task.CompletedTask;
    }
}
