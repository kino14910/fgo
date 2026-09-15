using Fgo.Scripts.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace Fgo.Scripts.Patches;

/// <summary>
///     联机皮肤同步（战斗生物视觉）。
///     【为什么挂在 Creature.CreateVisuals，而不是 CharacterModel.CreateVisuals】
///     游戏为"某位玩家"创建生物视觉的链路为：
///     NCreature.Create(creature) → creature.CreateVisuals() → Player.Character.CreateVisuals()
///     其中 <c>Player.Character</c> 是 <c>ModelDb</c> 里的"规范化单例" CharacterModel
///     （见 Player.CreateForNewRun → ModelDb.Character&lt;T&gt;()、FromSerializable → ModelDb.GetById），
///     **同角色的所有玩家共享同一个 CharacterModel 实例**。
///     因此若把 postfix 挂在 CharacterModel.CreateVisuals 上、再用
///     <c>Players.FirstOrDefault(p =&gt; p.Character == __instance)</c> 反查拥有者，会命中"所有玩家"
///     并恒返回第一个（主机）→ 把主机的皮肤套到每个玩家身上，表现为"主机和客机都显示主机皮肤"。
///     而 <see cref="Creature" /> 是"每位玩家一个"的实例，带有 <c>Creature.Player</c>（拥有者）
///     与 <c>Player.NetId</c>，能唯一定位"这条视觉属于谁"。故 postfix 改挂在此处。
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
public static class FgoCreatureSkinPatch
{
    // Creature（每位玩家唯一）-> 其已创建的视觉，供收到皮肤同步消息后即时重套
    private static readonly Dictionary<Creature, NCreatureVisuals> VisualsCache = new();

    public static void Postfix(Creature __instance, NCreatureVisuals? __result)
    {
        if (__result == null) return;

        // 只处理玩家生物：怪物的 Creature.Player 为 null，直接跳过。
        var player = __instance.Player;
        if (player is null) return;
        if (player.Character is not FgoCharacter) return;

        VisualsCache[__instance] = __result;

        // 按"拥有者的 NetId"取该玩家自己同步出去的皮肤（Sidecar 已缓存到 RemoteSkins）；
        // 未同步到时回退本机设置，保证单机 / 开局瞬间也不会丢皮肤。
        FgoSkinApplier.ApplySkinToCreature(__result, FgoSkinSync.ResolveSkin(player.NetId));
    }

    /// <summary>
    ///     收到某玩家的皮肤同步消息后，若该玩家的生物视觉已创建，立即按最新皮肤重套，
    ///     覆盖「视觉先于皮肤消息到达」的时序（例如后加入的队友、或者开局瞬间）。
    /// </summary>
    public static void ReapplySkin(ulong netId, int skin)
    {
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return;

        var player = runState.Players.FirstOrDefault(p => p.NetId == netId);
        if (player is null) return;
        if (!VisualsCache.TryGetValue(player.Creature, out var visuals)) return;

        FgoSkinApplier.ApplySkinToCreature(visuals, skin);
    }
}