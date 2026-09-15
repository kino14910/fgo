using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
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

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (QuartzCounter < CostPerChoice) return;
        var player = context.Player;

        // 与圣晶石一致：仅持有该遗物的本地玩家开启选择界面，其它端直接跳过。
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner))
            return;
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);

        var existing = CardPile.Get(FgoEnums.NobleDeck, player)?.Cards
            .Select(c => c.GetType())
            .ToHashSet() ?? [];

        // 候选 = 未在宝具卡组本局已拥有的 && 不在共享排除列表（见 FgoCardActions.ExcludedFromNobleDrawing）。
        var candidates = ModelDb.CardPool<NobleCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .OfType<NobleCardModel>()
            .Where(card => !existing.Contains(card.GetType()) &&
                           !FgoCardActions.ExcludedFromNobleDrawing.Contains(card.GetType()))
            .Select(card => player.RunState.CreateCard(card, player))
            .ToList();

        if (candidates.Count == 0)
        {
            Flash();
            return;
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
                context.PlayerChoiceContext!, candidates, player, prefs))
            .FirstOrDefault();

        if (selected == null) return;

        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile != null)
        {
            var result = await CardPileCmd.Add(selected, noblePile);

            // 联机同步：把「该玩家获得此宝具」广播给其它端，使其本地 NobleDeck 保持一致
            // （NobleDeck 为 RunPersistent 牌堆，运行期间本机改动不会自动传播）。
            if (result is { success: true })
            {
                FgoNobleDeckSync.NotifyAdd(player, selected.Id);

                // 同 SaintQuartz：右键消耗只在拥有者本机运行（非复制动作），QuartzCount 又在主机权威的
                // PlayerRunSavedData 上（客户端写不回传主机），故扣费后显式上报/广播新计数；
                // 放在“加卡成功”分支内，避免加卡失败却照扣圣晶石。
                QuartzCounter -= CostPerChoice;
                FgoQuartzSync.NotifyLocalCount(player);
            }

            RefreshQuartzActivationVisual(CostPerChoice);
            Flash();
            FgoCardActions.PreviewNoblePileAdd(result);
        }
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