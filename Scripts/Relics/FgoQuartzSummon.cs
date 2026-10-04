using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

/// <summary>
///     圣晶石 / 召唤券右键抽取宝具的公共流程。
///     <para>
///         抽取是「本地弹界面 → 玩家点选 → 把结果提交为托管动作」三段式：
///         前两段完全在同步动作之外（见 <see cref="Fgo.Scripts.Commands.FgoQuartzSummonCmd" /> 注释），
///         只有第三段进入官方动作队列，且不含任何等待玩家输入的 await，
///         因此不会占住 ActionExecutor，队友的地图投票等动作也不会被拖住。
///     </para>
/// </summary>
internal static class FgoQuartzSummon
{
    private static bool _choosing;

    /// <summary>当前能否提交抽取结果（界面应在弹出前先判断，避免选完才发现发不出去）。</summary>
    internal static bool CanRequestNow()
    {
        return FgoQuartzSummonCmd.CanRequestNow();
    }

    /// <summary>
    ///     抽取候选: 本局 NobleDeck 尚未拥有的、且不在共享排除列表里的宝具卡。
    ///     用 RunState.CreateCard（非 CombatState），使右键在地图上也能使用。
    /// </summary>
    internal static List<CardModel> BuildCandidates(Player player)
    {
        var existing = CardPile.Get(FgoEnums.NobleDeck, player)?
            .Cards
            .Select(c => c.GetType())
            .ToHashSet() ?? [];

        return ModelDb.CardPool<NobleCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .OfType<NobleCardModel>()
            .Where(card => !existing.Contains(card.GetType()) &&
                           !FgoCardActions.ExcludedFromNobleDrawing.Contains(card.GetType()))
            .Select(card => (CardModel)player.RunState.CreateCard(card, player))
            .ToList();
    }

    /// <summary>
    ///     在同步动作之外启动抽取流程: flow 内部等待玩家点击的 await 只挂住本地流程，
    ///     不会让 GameAction 滞留在队列里。本地同一时刻只允许一个抽取界面。
    /// </summary>
    internal static void Begin(Func<Task> flow)
    {
        if (_choosing) return;
        _choosing = true;
        TaskHelper.RunSafely(Guarded(flow));
    }

    private static async Task Guarded(Func<Task> flow)
    {
        try
        {
            await flow();
        }
        catch (Exception ex)
        {
            // 纯本地的展示流程（含界面本身）失败不得影响牌局；异常多半来自 Godot 节点已释放。
            Entry.Logger.Warn($"[Fgo] quartz summon flow failed: {ex}");
        }
        finally
        {
            _choosing = false;
        }
    }

    /// <summary>把选中的宝具提交为托管动作，各端按 Id 确定性加卡并扣除计数。</summary>
    internal static bool Submit(Player player, CardModel selected)
    {
        return FgoQuartzSummonCmd.Request(player, selected.Id);
    }
}
