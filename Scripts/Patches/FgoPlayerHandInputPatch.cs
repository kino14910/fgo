using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching;
using STS2RitsuLib.Patching.Models;

namespace Fgo.Scripts.Patches;

/// <summary>
///     原版 <see cref="NPlayerHand._UnhandledInput" /> 的越界护栏。
/// </summary>
/// <remarks>
///     <para>
///         原版在方法开头执行 <c>list.Insert(_draggedHolderIndex, null)</c>，给拖拽中的牌留出空位，
///         索引取自开始拖拽那一刻 holder 在 <c>CardHolderContainer</c> 里的下标。
///         〔虚数空间〕进场会把原版手牌整体搬进无界面暂存牌堆，并由 <c>FgoGlobalHud</c> 隐藏整个
///         <c>CardHolderContainer</c>（<c>IsVisibleInTree()</c> 变假 ⇒ <c>ActiveHolders</c> 归零），
///         于是「拖牌出牌中」这个窗口里任何输入事件（哪怕只是鼠标移动）都会让 Insert 的索引
///         大于列表长度，抛出 <see cref="System.ArgumentOutOfRangeException" />——
///         日志里每次打出〔虚数大海战〕后紧跟的那条 ERROR 就是它。
///     </para>
///     <para>
///         这里在原版执行前拦掉越界的那一次输入: 索引失效时快捷键选牌本来也选不到正确的牌
///         （手牌已被搬空/隐藏），跳过没有功能损失；索引正常时一律放行。
///     </para>
/// </remarks>
public sealed class FgoPlayerHandInputPatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NPlayerHand, int> DraggedHolderIndexRef =
        PrivateAccess.FieldRef<NPlayerHand, int>("_draggedHolderIndex");

    public static string PatchId => "fgo.player_hand_input_guard";

    public static string Description =>
        "Skip NPlayerHand._UnhandledInput while the dragged-holder index is stale (void space hand swap)";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [PatchTarget.Method<NPlayerHand>(nameof(NPlayerHand._UnhandledInput))];
    }

    [HarmonyPrefix]
    private static bool Prefix(NPlayerHand __instance)
    {
        // 原版 list.Insert 的合法区间是 [0, ActiveHolders.Count]; -1 表示没有拖拽中的牌。
        var draggedIndex = DraggedHolderIndexRef(__instance);
        return draggedIndex < 0 || draggedIndex <= __instance.ActiveHolders.Count;
    }
}