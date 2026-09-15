using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.Patches;

/// <summary>
///     商店（商人房间）角色皮肤：按<b>每个形象各自的拥有者</b>套皮肤。
///     【为什么必须自己挂 patch，而不能只靠 <c>SetupCustomMerchantAnimationStateMachine</c>】
///     <c>NMerchantCharacter</c> 本身<b>不携带</b>拥有者（没有 Player 属性），
///     而 <c>NMerchantRoom.AfterRoomIsLoaded</c> 只为形象准备了一份共享的 <c>CharacterModel</c>
///     （同角色所有玩家共用一个单例，无法区分玩家）。
///     而且 Ritsu 的皮肤入口是 <c>PlayAnimation</c> prefix 里"惰性"构建状态机时才顺带调用的，
///     一旦这条路径没走到（或走的时候节点还没挂进 <c>PlayerVisuals</c>），
///     精灵就会停在 <c>guda_merchant.tscn</c> 自带的默认贴图 Master15.png —— 即"回到默认值"。
///     这里改为在房间加载完成后统一按 <c>_players[k] &lt;-&gt; PlayerVisuals[k]</c> 的下标对应关系
///     （见 <c>NMerchantRoom.AfterRoomIsLoaded</c>：两者按同一个下标顺序添加）逐个套皮肤，
///     时机确定、多人下每个形象各归各主。
/// </summary>
[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
public static class FgoMerchantSkinPatch
{
    public static void Postfix(NMerchantRoom __instance)
    {
        FgoWorldSkin.ApplyMerchantRoom(__instance);
    }
}
