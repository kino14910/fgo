using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Utils;

namespace Fgo.Scripts.Fields;

/// <summary>
///     场地状态的唯一入口: 读取与修改场地一律走这里，外部不要自行维护一份副本。
/// </summary>
/// <remarks>
///     状态按 <see cref="ICombatState" /> 附着（<c>AttachedState</c> 基于 ConditionalWeakTable），
///     战斗对象回收即自动清理，无需手动重置，也不会跨战斗残留。
///     <para />
///     键类型用接口 <see cref="ICombatState" /> 而非 <c>CombatState</c>，是为了让
///     <c>Creature.CombatState</c>（返回接口）与 <c>CurrentCombatState</c>（返回实现类）都能直接传入；
///     ConditionalWeakTable 按引用判等，两者指向同一对象，不会产生两份状态。
///     <para />
///     <b>跨端一致性</b>: 目前所有场地写入都发生在被复制的游戏动作内部（出牌 OnPlay、回合钩子），
///     各端会各自执行同一份逻辑，因此无需额外的 Sidecar 广播。
///     一旦出现「只在本地发起」的场地改动（例如 UI 按钮、右键交互），必须补一层版本化快照广播，
///     否则该场地只在本机生效，进而导致各端状态分叉。
///     <para />
///     <b>关于返回 <c>Task</c></b>: 写入方法返回 <c>Task</c> 只是为了与项目里其它效果 API
///     （<c>PlayerCmd.LoseEnergy</c> 等）保持一致的调用形状——调用方一律 <c>await</c>，
///     读起来不会出现"别的都是 await、只有场地是裸调用"的割裂。
///     <b>但返回的 Task 恒为已完成，这些方法内部绝不能引入真正的 <c>await</c>。</b>
///     原因有二:
///     ① 调用点普遍是「连续写入 → 紧接着读取」的模式（如 <c>GreatVoidSeaBattle</c> 连加两个场地后立刻
///     <c>EnterAll</c>；<c>FgoFieldEffects.OnPlayerSideTurnStart</c> <c>TickDown</c> 之后马上按 <c>Has</c> 结算），
///     一旦中间真的让出，游戏循环可能插进来跑一帧，逐帧轮询的 HUD / 背景就会观察到"只写了一半"的状态；
///     ② 这些写入发生在被复制的游戏动作内，各端各跑一遍，同步落地才能保证各端序列一致。
///     真正的异步（需要 <c>PlayerChoiceContext</c> 的引擎调用）请放在 <see cref="FgoFieldEffects" /> 里，
///     由**调用方**在拿到 <c>context</c> 后自行 await。
/// </remarks>
public static class FgoField
{
    private static readonly AttachedState<ICombatState, FgoFieldState> States =
        new(() => new FgoFieldState());

    /// <summary>取该战斗的场地表；战斗为空或尚无任何场地改动时返回 null。</summary>
    public static FgoFieldState? Of(ICombatState? combat) =>
        combat == null ? null : States.GetValueOrDefault(combat);

    public static bool Has(ICombatState? combat, FgoFieldId id) => Of(combat)?.Has(id) == true;

    /// <summary>该场地的层数；不存在时返回 0。</summary>
    public static int StacksOf(ICombatState? combat, FgoFieldId id) => Of(combat)?.StacksOf(id) ?? 0;

    public static IReadOnlyCollection<FgoFieldId> ActiveOf(ICombatState? combat) =>
        Of(combat)?.Active ?? [];

    /// <summary>场地集合版本号；UI 用它与上次渲染的版本比对做脏检查。</summary>
    public static int VersionOf(ICombatState? combat) => Of(combat)?.Version ?? 0;

    /// <summary>
    ///     叠加场地层数（= 持续回合数）。已存在则累加，因此重复施加可以延长同一个场地。
    ///     回合数由施加方（各张卡牌）自行给出，这里不做统一。
    /// </summary>
    /// <returns>是否发生了实际变化。返回的 Task 恒为已完成，见类型 remarks。</returns>
    public static Task<bool> Add(ICombatState? combat, FgoFieldId id, int stacks = 1) =>
        Task.FromResult(combat != null && States.GetOrCreate(combat).Add(id, stacks));

    /// <summary>移除场地。原本不存在时返回 false。</summary>
    public static Task<bool> Remove(ICombatState? combat, FgoFieldId id) =>
        Task.FromResult(combat != null && States.GetValueOrDefault(combat) is { } state && state.Remove(id));

    /// <summary>每回合开始时调用: 对所有「有时限」的场地各 -1 层，归零即移除。</summary>
    public static Task TickDown(ICombatState? combat)
    {
        if (combat == null) return Task.CompletedTask;
        States.GetValueOrDefault(combat)?.TickDown();
        return Task.CompletedTask;
    }

    public static Task Clear(ICombatState? combat)
    {
        if (combat == null) return Task.CompletedTask;
        States.GetValueOrDefault(combat)?.Clear();
        return Task.CompletedTask;
    }
}