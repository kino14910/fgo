using Fgo.Scripts.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace Fgo.Scripts.Patches;

/// <summary>
///     领袖气质（<see cref="CharismaPower" />）：持有该能力时，能量不足也可打出卡牌。
///     <para>
///         官方 <see cref="PlayerCombatState.HasEnoughResourcesFor" /> 会在能量不够时把
///         <see cref="UnplayableReason.EnergyCostTooHigh" /> 写进 reason，进而让 <c>CardModel.CanPlay</c> 返回 false，
///         卡牌无法被点出。这里对持有 <see cref="CharismaPower" /> 的玩家清除该 flag：
///         缺失的能量改由该能力在 <c>BeforeCardPlayed</c> 中以生命值 + 疲劳结算。
///     </para>
///     <para>
///         门槛挂在**能力**上而不是某张卡上，所以能力被移除后立刻恢复正常出牌规则。
///         其余不可打出原因（Unplayable 关键词、BlockedByHook、StarCostTooHigh 等）保持原样。
///     </para>
/// </summary>
public sealed class FgoCharismaCostPatch : IPatchMethod
{
    public static string PatchId => "fgo.charisma.energy_shortfall";

    public static string Description => "Allow cards to be played with insufficient energy while Charisma is active";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            PatchTarget.Method<PlayerCombatState>(nameof(PlayerCombatState.HasEnoughResourcesFor))
        ];
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel card, ref UnplayableReason reason, ref bool __result)
    {
        if (!reason.HasFlag(UnplayableReason.EnergyCostTooHigh)) return;
        if (card.Owner?.Creature is not { } creature) return;
        if (!creature.HasPower<CharismaPower>()) return;

        reason &= ~UnplayableReason.EnergyCostTooHigh;
        __result = reason == UnplayableReason.None;
    }
}
