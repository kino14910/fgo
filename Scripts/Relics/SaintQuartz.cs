using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

/// <summary>
///     圣晶石: FGO 角色的初始遗物，仅负责圣晶石计数与右键消耗计数换取宝具卡。
///     - 每进入一个房间（每层）+1 计数。
///     - 计数 ≥ 3 时，可右键此遗物从尚未在 NobleDeck 的宝具卡中随机抽取 3 张作为候选，
///     弹出与卡牌奖励一致的三选一界面，由玩家手动选择其中 1 张加入 NobleDeck，并消耗 3 计数。
///     - 被 [gold]点金石[/gold] 祝福后升级为[gold]召唤券[/gold]（SummonTicket），可在全部候选中手动挑选要加入的宝具卡。
///     - 计数存于按玩家的 FgoRunState（见 FgoRelic.QuartzCounter）: 升级替换遗物实例后计数保留。
///     - NobleDeck 牌堆的生命周期（播种初始宝具卡、按钮绑定）已与此遗物解耦，
///     由 run 生命周期在 Entry 中统一处理（见 FgoCardActions.EnsureNobleDeckSeeded）。
///     - 抽取界面在同步动作之外打开（见 FgoQuartzSummon / FgoQuartzSummonCmd）:
///     右键本身仍是一个同步动作，但它只负责「发起」，绝不等待玩家点击。
/// </summary>
[RegisterCharacterStarterRelic(typeof(FgoCharacter))]
public class SaintQuartz : FgoRelic, IModRightClickableRelic
{
    public const int CostPerChoice = 3;

    internal static bool RightClickSuppressed { get; set; }

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;
    public override int DisplayAmount => QuartzCounter;

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        if (RightClickSuppressed) return false;
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner)) return false;
        return QuartzCounter >= CostPerChoice;
    }

    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        if (RightClickSuppressed) return false;
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner)) return false;
        return QuartzCounter >= CostPerChoice;
    }

    /// <summary>
    ///     右键触发: 从 NobleCardPool 中尚未在 NobleDeck 的宝具卡里随机抽取 3 张（候选不足 3 张时按实际数量）
    ///     作为本次抽取的选项，弹出与卡牌奖励一致的三选一界面，由玩家手动选择 1 张加入 NobleDeck。
    ///     <para>
    ///         本方法是同步动作（RitsuLib 托管右键）的执行体，因此【只发起、不等待】：
    ///         界面由 <see cref="FgoQuartzSummon.Begin" /> 在动作之外打开，
    ///         玩家点选后由 <see cref="FgoQuartzSummonCmd" /> 广播结果。
    ///         若在同步动作里直接 await 界面选择，该动作将永不结束，而 ActionExecutor 全局串行 →
    ///         所有玩家（含队友）的后续动作都排不上，表现为「点了圣晶石后点房间 → 队友黑屏卡死」。
    ///     </para>
    /// </summary>
    public Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (QuartzCounter < CostPerChoice) return Task.CompletedTask;
        if (RightClickSuppressed) return Task.CompletedTask;

        var player = context.Player;

        // 多人模式：界面是纯本地 UI，仅由拥有该圣晶石的本地玩家开启，其余端直接跳过。
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner))
            return Task.CompletedTask;

        FgoQuartzSummon.Begin(() => SummonFlow(player));
        return Task.CompletedTask;
    }

    /// <summary>
    ///     三选一界面流程（运行在同步动作之外）: 随机 3 张候选 → 玩家点选 → 提交结果。
    ///     这里的 await 只挂住本地流程，不会让任何 GameAction 滞留在队列里。
    /// </summary>
    private async Task SummonFlow(Player player)
    {
        var candidates = FgoQuartzSummon.BuildCandidates(player);
        if (candidates.Count == 0)
        {
            Flash();
            return;
        }

        // 注意：候选选项的随机抽取必须使用【非同步】RNG（Rng.Chaotic）。
        // CombatCardSelection / CombatCardGeneration 等 RunState.Rng.* 是联机网络的权威同步 RNG，
        // 仅在“被复制的游戏动作”内部消耗才安全。本流程只在拥有者的本机运行，
        // 若在本地直接消耗同步 RNG，会让各端 CombatCardSelection 计数永久分叉，触发“状态分歧”断线。
        // 三个候选仅用于本地展示，最终加入哪张卡由 FgoQuartzSummonCmd 按 Id 广播，各端结果一致。
        var options = candidates
            .TakeRandom(Math.Min(3, candidates.Count), Rng.Chaotic)
            .Select(c => new CardCreationResult(c))
            .ToList();

        // 此刻提交动作无法入队（例如战斗中非出牌阶段）→ 不弹界面，避免选完才发现发不出去。
        if (!FgoQuartzSummon.CanRequestNow())
        {
            Flash();
            return;
        }

        var screen = NCardRewardSelectionScreen.ShowScreen(options, Array.Empty<CardRewardAlternative>());
        if (screen == null)
        {
            Flash();
            return;
        }

        int? chosen;
        try
        {
            chosen = await screen.OptionSelected();
        }
        catch (TaskCanceledException)
        {
            // 玩家未选择就关闭界面（界面节点销毁时会取消该 await）：不扣费、不加卡。
            return;
        }

        if (chosen is not { } idx || idx < 0 || idx >= options.Count) return;

        var selected = options[idx].Card;

        // 先把卡节点移出界面，避免界面移除时连带释放、也让它能用于飞行表现。
        var holder = screen.GetCardHolder(selected);
        var cardNode = holder?.CardNode;
        if (cardNode != null)
            NRun.Instance?.GlobalUi?.ReparentCard(cardNode);
        holder?.QueueFreeSafely();
        NOverlayStack.Instance?.Remove(screen);

        if (!FgoQuartzSummon.Submit(player, selected))
        {
            // 提交失败：本次抽取作废，界面已关，仅做一次闪烁提示。
            cardNode?.QueueFreeSafely();
            Flash();
            return;
        }

        Flash();

        // 飞入牌堆的表现是纯装饰：结果由托管动作在各端分别结算，这里只播放本地动画。
        if (cardNode != null && NRun.Instance?.GlobalUi?.TopBar?.TrailContainer is { } trail)
            trail.AddChildSafely(
                NCardFlyVfx.Create(cardNode, FgoEnums.NobleDeck, true, player.Character.TrailPath));
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        // 读档重建当前房间时（仅客户端）重放本动作：已在存档里计过，跳过以免相对主机多 +1。
        // 正常推进/单机/主机：各端本地自增，与原版 QuartzCounter++ 行为一致。
        if (FgoQuartzSync.ShouldSkipRoomEntry()) return Task.CompletedTask;
        IncrementQuartzOnRoomEntry(CostPerChoice);
        return Task.CompletedTask;
    }
}