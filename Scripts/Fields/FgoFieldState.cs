namespace Fgo.Scripts.Fields;

/// <summary>
///     单场战斗的场地表: 战场上的场地及其层数。
/// </summary>
/// <remarks>
///     层数 = 剩余回合数: 所有场地都是有时限的（回合数由施加方给出），每回合开始时 -1，归零即消失。
///     是否递减由 <see cref="FgoFieldIdExtensions.TicksDownEachTurn" /> 决定。
///     <para />
///     状态按战斗对象附着（见 <see cref="FgoField" />），战斗结束随对象一并回收。
/// </remarks>
public sealed class FgoFieldState
{
    private readonly Dictionary<FgoFieldId, int> _stacks = [];

    /// <summary>
    ///     场地集合的版本号: 每次增删或层数变化自增。供 UI 做脏检查，避免每帧重建显示。
    /// </summary>
    public int Version { get; private set; }

    public IReadOnlyCollection<FgoFieldId> Active => _stacks.Keys;

    public bool Has(FgoFieldId id) => _stacks.GetValueOrDefault(id) > 0;

    public int StacksOf(FgoFieldId id) => _stacks.GetValueOrDefault(id);

    /// <summary>叠加层数。已存在则累加；累加到 0 或负数等价于移除。</summary>
    public bool Add(FgoFieldId id, int stacks)
    {
        var target = _stacks.GetValueOrDefault(id) + stacks;
        if (target <= 0) return Remove(id);
        if (_stacks.GetValueOrDefault(id) == target) return false;

        _stacks[id] = target;
        Version++;
        return true;
    }

    /// <summary>移除场地。原本不存在时返回 false。</summary>
    public bool Remove(FgoFieldId id)
    {
        if (!_stacks.Remove(id)) return false;
        Version++;
        return true;
    }

    /// <summary>
    ///     每回合开始时调用: 对所有「有时限」的场地各 -1 层，归零即移除。
    /// </summary>
    /// <returns>是否有任何场地发生变化。</returns>
    public bool TickDown()
    {
        if (_stacks.Count == 0) return false;

        // 先快照键集合: 递减过程中可能移除条目，不能边遍历边改字典。
        var changed = false;
        foreach (var id in _stacks.Keys.ToList())
        {
            if (!id.TicksDownEachTurn()) continue;

            var left = _stacks[id] - 1;
            if (left > 0) _stacks[id] = left;
            else _stacks.Remove(id);
            changed = true;
        }

        if (changed) Version++;
        return changed;
    }

    public void Clear()
    {
        if (_stacks.Count == 0) return;
        _stacks.Clear();
        Version++;
    }
}
