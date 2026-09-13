using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
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
/// </summary>
[RegisterCharacterStarterRelic(typeof(FgoCharacter))]
public class SaintQuartz : FgoRelic, IModRightClickableRelic
{
    public const int CostPerChoice = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;
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
    ///     右键触发: 从 NobleCardPool 中尚未在 NobleDeck 的宝具卡里随机抽取 3 张（候选不足 3 张时按实际数量）
    ///     作为本次抽取的选项，弹出与卡牌奖励一致的三选一界面，由玩家手动选择 1 张加入 NobleDeck。
    /// </summary>
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (QuartzCounter < CostPerChoice) return;

        var player = context.Player;

        // 多人模式：圣晶石右键抽取是组内同步操作。若让每位队友的机器都执行本方法，
        // 会各自弹出三选一界面；队友的选择会以错误权威在主机侧把卡加入 NobleDeck 失败
        // （表现为"队友也能选但不进牌库"）。因此仅由拥有该圣晶石的本地玩家在本机开启选择界面，
        // 其余端直接跳过，既不弹界面也不产生无效加入。
        if (FgoConfigSync.IsNetworkedRun() && !LocalContext.IsMe(Owner))
            return;

        var existing = CardPile.Get(FgoEnums.NobleDeck, player)?.Cards
            .Select(c => c.GetType())
            .ToHashSet() ?? [];

        // 候选 = 未在宝具卡组本局已拥有的 && 不在共享排除列表（见 FgoCardActions.ExcludedFromNobleDrawing）。
        // 用 RunState.CreateCard 而非 CombatState.CreateCard，使右键在地图上也能使用。
        // NobleDeck 是 RunPersistent 牌堆，加入的卡不进入战斗 pile，无需注册到 CombatState。
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

        // 注意：候选选项的随机抽取必须使用【非同步】RNG（Rng.Chaotic）。
        // CombatCardSelection / CombatCardGeneration 等 RunState.Rng.* 是联机网络的权威同步 RNG，
        // 仅在“被复制的游戏动作”内部消耗才安全。本方法只在拥有者的本机运行（其余端在上方已 return），
        // 若在本地直接消耗同步 RNG，会让各端 CombatCardSelection 计数永久分叉，触发“状态分歧”断线。
        // 三个候选仅用于本地展示，最终选中的宝具由下方 CardPileCmd.Add（同步指令）复制给所有端，
        // 因此用 Chaotic（装饰/UI 用的非同步 RNG，见 FgoNpGainVfx）即可，无需各端一致。
        var options = candidates
            .TakeRandom(Math.Min(3, candidates.Count), Rng.Chaotic)
            .Select(c => new CardCreationResult(c))
            .ToList();

        var screen = NCardRewardSelectionScreen.ShowScreen(options, Array.Empty<CardRewardAlternative>());
        int? chosen;
        if (screen != null)
        {
            chosen = await screen.OptionSelected();
            NOverlayStack.Instance?.Remove(screen);
        }
        else
        {
            chosen = 0;
        }

        if (chosen is not { } idx) return;

        var selected = options[idx].Card;
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile != null)
        {
            var holder = screen?.GetCardHolder(selected);
            var result = await CardPileCmd.Add(selected, noblePile);

            // 联机同步：NobleDeck 是 RunPersistent 牌堆，运行期间本机改动不会实时传播到其它端。
            // 把「该玩家获得此宝具」广播出去，让主机与其它队友在本地 NobleDeck 内补记同一张卡，
            // 否则后续 np_button 托管动作各端按不同候选重放 → 手牌分歧断线。
            if (result is { success: true })
            {
                FgoNobleDeckSync.NotifyAdd(player, selected.Id);

                // 扣费：右键抽取只在拥有者本机运行（非复制动作），而 QuartzCount 存于主机权威的
                // PlayerRunSavedData（客户端写不回传主机）——不上报则主机侧不扣、且下次 BroadcastAll
                // 会把已扣值覆盖回旧值。故扣费后显式把新计数上报/广播。放在“加卡成功”分支内，
                // 避免加卡失败却照扣圣晶石。
                QuartzCounter -= CostPerChoice;
                FgoQuartzSync.NotifyLocalCount(player);
            }

            RefreshQuartzActivationVisual(CostPerChoice);
            Flash();

            if (holder != null && result is { success: true })
            {
                var cardNode = holder.CardNode;
                NRun.Instance.GlobalUi.ReparentCard(cardNode);
                holder.QueueFreeSafely();
                NRun.Instance.GlobalUi.TopBar.TrailContainer.AddChildSafely(
                    NCardFlyVfx.Create(cardNode, result.cardAdded.Pile.Type, true,
                        result.cardAdded.Owner.Character.TrailPath));
            }
            else
            {
                FgoCardActions.PreviewNoblePileAdd(result);
            }

            NOverlayStack.Instance?.Remove(screen);
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