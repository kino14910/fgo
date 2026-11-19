using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;

namespace Fgo.Scripts.Relics;

/// <summary>
///     圣晶石 / 呼符抽取宝具的共享逻辑：候选构造、本地门控与结算。
///     选择与结算都发生在 RitsuLib 右键托管动作内，各端按队列顺序重放。
/// </summary>
internal static class FgoQuartzSummon
{
    /// <summary>冥界终局事件期间禁止抽取，避免玩家绕过「献上 3 圣晶石」选项。</summary>
    internal static bool RightClickSuppressed { get; set; }

    /// <summary>
    ///     仅在本地预检调用：地图 / capstone 是本地 UI 状态，放进各端重放的动作会造成状态分歧。
    ///     地图打开时 NOverlayStack.HideOverlays 会隐藏选卡界面，点击会穿透到地图节点。
    /// </summary>
    internal static bool CanOpenSelection()
    {
        if (RightClickSuppressed) return false;
        if (NMapScreen.Instance?.IsOpen == true) return false;
        if (NCapstoneContainer.Instance?.InUse == true) return false;
        return true;
    }

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
            .Select(card => player.RunState.CreateCard(card, player))
            .ToList();
    }

    /// <summary>
    ///     候选在各端复制的动作内构造，随机源必须确定性。种子只用 run / 玩家槽位 / 遗物 Id / 楼层，
    ///     不掺入可能各端滞后的计数；也不消耗 CombatCardSelection 等权威同步 RNG 流。
    /// </summary>
    internal static List<CardModel> BuildOptions(Player player, RelicModel relic, int count)
    {
        var candidates = BuildCandidates(player);
        if (candidates.Count <= count) return candidates;
        var rng = new Rng(player, relic.Id, (ulong)player.RunState.TotalFloor);
        return candidates.TakeRandom(count, rng).ToList();
    }

    internal static async Task<bool> Settle(Player player, CardModel selected)
    {
        if (ModelDb.GetByIdOrNull<NobleCardModel>(selected.Id) is not { } canonical)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: unknown noble card '{selected.Id}'.");
            return false;
        }

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: NobleDeck missing for netId={player.NetId}.");
            return false;
        }

        if (pile.Cards.Any(c => c.Id == canonical.Id))
        {
            Entry.Logger.Warn(
                $"[Fgo] quartz summon skipped: {canonical.Id.Entry} already in NobleDeck of netId={player.NetId}.");
            return false;
        }

        var relic = FindQuartzRelic(player);
        if (relic == null)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: no quartz relic for netId={player.NetId}.");
            return false;
        }

        if (relic.QuartzCount < SaintQuartz.CostPerChoice)
        {
            Entry.Logger.Warn(
                $"[Fgo] quartz summon rejected: netId={player.NetId} count={relic.QuartzCount} < {SaintQuartz.CostPerChoice}.");
            return false;
        }

        var card = player.RunState.CreateCard(canonical, player);
        var result = await CardPileCmd.Add(card, pile);
        if (result is not { success: true })
        {
            Entry.Logger.Warn(
                $"[Fgo] quartz summon failed: could not add {canonical.Id.Entry} for netId={player.NetId}.");
            return false;
        }

        relic.SpendQuartz(SaintQuartz.CostPerChoice);

        // NobleDeck 是 RunPersistent 牌堆，运行期改动不自动传播。
        FgoNobleDeckSync.NotifyAdd(player, card.Id);
        FgoQuartzSync.NotifyLocalCount(player);

        Entry.Logger.Info($"[Fgo] quartz summon: netId={player.NetId} gained {card.Id.Entry}");
        return true;
    }

    private static FgoRelic? FindQuartzRelic(Player player)
    {
        return (FgoRelic?)player.GetRelic<SaintQuartz>() ?? player.GetRelic<SummonTicket>();
    }
}