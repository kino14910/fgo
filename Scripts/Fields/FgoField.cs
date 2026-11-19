using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using STS2RitsuLib.Utils;

namespace Fgo.Scripts.Fields;

/// <remarks>写入方法返回的 Task 恒已完成，内部绝不能真 await：调用点连续写入后紧接读取，让出会让 HUD 看到半状态、各端序列分叉。</remarks>
public static class FgoField
{
    public const int ActFieldTurns = 3;

    private static readonly AttachedState<ICombatState, FgoFieldState> States =
        new(() => new FgoFieldState());

    public static FgoFieldId? FieldForAct(ActModel? act)
    {
        switch (act)
        {
            case Underdocks:
                return FgoFieldId.Waterside;
            case Overgrowth:
                return FgoFieldId.Forest;
            case Hive:
                return FgoFieldId.Darkness;
            case Glory:
                return FgoFieldId.City;
            default:
                return null;
        }
    }

    public static FgoFieldState? Of(ICombatState? combat)
    {
        return combat == null ? null : States.GetValueOrDefault(combat);
    }

    public static bool Has(ICombatState? combat, FgoFieldId id)
    {
        return Of(combat)?.Has(id) == true;
    }

    public static int StacksOf(ICombatState? combat, FgoFieldId id)
    {
        return Of(combat)?.StacksOf(id) ?? 0;
    }

    public static IReadOnlyCollection<FgoFieldId> ActiveOf(ICombatState? combat)
    {
        return Of(combat)?.Active ?? [];
    }

    public static int VersionOf(ICombatState? combat)
    {
        return Of(combat)?.Version ?? 0;
    }

    /// <summary>已存在则累加，重复施加可延长同一场地；回合数由施加方给出。</summary>
    public static Task<bool> Add(ICombatState? combat, FgoFieldId id, int stacks = 1)
    {
        return Task.FromResult(combat != null && States.GetOrCreate(combat).Add(id, stacks));
    }

    public static Task<bool> Remove(ICombatState? combat, FgoFieldId id)
    {
        return Task.FromResult(combat != null && States.GetValueOrDefault(combat) is { } state && state.Remove(id));
    }

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