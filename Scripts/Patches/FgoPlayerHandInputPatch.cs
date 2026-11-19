using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching;
using STS2RitsuLib.Patching.Models;

namespace Fgo.Scripts.Patches;

/// <summary>
///     原版 <see cref="NPlayerHand._UnhandledInput" /> 的越界护栏。
/// </summary>
/// <remarks>虚数空间进场隐藏手牌容器使 ActiveHolders 归零，拖牌期间 list.Insert 索引越界抛 ArgumentOutOfRange；这里在原版前拦掉该次输入。</remarks>
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
        var draggedIndex = DraggedHolderIndexRef(__instance);
        return draggedIndex < 0 || draggedIndex <= __instance.ActiveHolders.Count;
    }
}