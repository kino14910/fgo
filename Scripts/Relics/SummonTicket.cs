using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

/// <summary>
///     召唤券: 圣晶石被 [gold]点金石[/gold] 祝福后的升级版。
///     保留了每层 +1 计数的被动，但右键弹出候选网格后可手动选择加入哪张宝具卡。
///     计数与圣晶石共享同一个按玩家的 FgoRunState 槽位（见 FgoRelic.QuartzCounter），
///     替换圣晶石后计数原样保留。
/// </summary>
[RegisterRelic(typeof(FgoRelicPool))]
public class SummonTicket : FgoRelic, IModRightClickableRelic
{
    public const int CostPerChoice = 3;

    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool ShowCounter => true;
    public override int DisplayAmount => QuartzCounter;

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner)) return false;
        return QuartzCounter >= CostPerChoice;
    }

    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner)) return false;
        return QuartzCounter >= CostPerChoice;
    }

    /// <summary>
    ///     右键触发: 弹出全部候选的网格界面，玩家手动挑选一张加入 NobleDeck 并消耗 3 计数。
    ///     <para>
    ///         与圣晶石一致：本方法是同步动作的执行体，只发起、不等待（界面在动作之外打开，
    ///         选完由 <see cref="FgoQuartzSummonCmd" /> 广播结果），
    ///         避免等待玩家点击把 ActionExecutor 全局堵死。
    ///     </para>
    /// </summary>
    public Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (QuartzCounter < CostPerChoice) return Task.CompletedTask;
        var player = context.Player;

        // 与圣晶石一致：仅持有该遗物的本地玩家开启选择界面，其它端直接跳过。
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner))
            return Task.CompletedTask;

        FgoQuartzSummon.Begin(() => SummonFlow(player));
        return Task.CompletedTask;
    }

    private async Task SummonFlow(Player player)
    {
        var candidates = FgoQuartzSummon.BuildCandidates(player);
        if (candidates.Count == 0)
        {
            Flash();
            return;
        }

        if (!FgoQuartzSummon.CanRequestNow())
        {
            Flash();
            return;
        }

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        var screen = NSimpleCardSelectScreen.Create(candidates, prefs);
        var overlayStack = NOverlayStack.Instance;
        if (overlayStack == null)
        {
            Flash();
            return;
        }

        overlayStack.Push(screen);

        CardModel? selected;
        try
        {
            selected = (await screen.CardsSelected()).FirstOrDefault();
        }
        catch (TaskCanceledException)
        {
            // 未选择就关闭界面：不扣费、不加卡。
            return;
        }

        overlayStack.Remove(screen);
        if (selected == null) return;

        if (!FgoQuartzSummon.Submit(player, selected))
        {
            Flash();
            return;
        }

        Flash();
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