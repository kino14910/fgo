namespace Fgo.Scripts.Fields;

/// <remarks>是否递减由 <see cref="FgoFieldIdExtensions.TicksDownEachTurn" /> 决定。状态按战斗对象附着（见 <see cref="FgoField" />），随战斗回收。</remarks>
public sealed class FgoFieldState
{
    private readonly Dictionary<FgoFieldId, int> _stacks = [];

    public int Version { get; private set; }

    public IReadOnlyCollection<FgoFieldId> Active => _stacks.Keys;

    /// <summary>本回合首次攻击牌是否已返还过活力；回合开始重置。</summary>
    public bool SunlightAttackPlayedThisTurn { get; set; }

    public bool Has(FgoFieldId id)
    {
        return _stacks.GetValueOrDefault(id) > 0;
    }

    public int StacksOf(FgoFieldId id)
    {
        return _stacks.GetValueOrDefault(id);
    }

    public bool Add(FgoFieldId id, int stacks)
    {
        var target = _stacks.GetValueOrDefault(id) + stacks;
        if (target <= 0) return Remove(id);
        if (_stacks.GetValueOrDefault(id) == target) return false;

        _stacks[id] = target;
        Version++;
        return true;
    }

    public bool Remove(FgoFieldId id)
    {
        if (!_stacks.Remove(id)) return false;
        Version++;
        return true;
    }

    public bool TickDown()
    {
        if (_stacks.Count == 0) return false;

        // 先快照键：递减中会移除条目。
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