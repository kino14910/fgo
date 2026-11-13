using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;

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
/// <remarks>
///     通过 RitsuLib 的 IPatchMethod 模式注册（见 Entry.Init）。原先是 [HarmonyPatch] 属性式 patch，
///     依赖 Entry.Init 里的 Harmony.PatchAll；0.113 适配时该注册入口被移除，属性式 patch 不再被扫描。
///     0.113 把商家房间拆成 NStandardMerchantRoom / NBlitzMerchantRoom，但两者的 _Ready 都调用
///     base._Ready()，基类 NMerchantRoom._Ready 仍会调用私有的 AfterRoomIsLoaded，因此 patch 基类
///     方法体即可同时覆盖两种房间；私有方法用字符串字面量声明目标（同 NobleCardPool 的 NCard.Reload）。
/// </remarks>
public sealed class FgoMerchantSkinPatch : IPatchMethod
{
    public static string PatchId => "fgo.skin.merchant_room";

    public static string Description => "Apply per-owner skins to merchant room character visuals";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [PatchTarget.Method<NMerchantRoom>("AfterRoomIsLoaded")];
    }

    [HarmonyPostfix]
    public static void Postfix(NMerchantRoom __instance)
    {
        FgoWorldSkin.ApplyMerchantRoom(__instance);
    }
}