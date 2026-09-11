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
    private const int CostPerChoice = 3;

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

        var options = candidates
            .TakeRandom(Math.Min(3, candidates.Count), player.RunState.Rng.CombatCardSelection)
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
            QuartzCounter -= CostPerChoice;
            UpdateAvailableVisual(CostPerChoice);
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
        QuartzCounter++;
        UpdateAvailableVisual(CostPerChoice);
        return Task.CompletedTask;
    }
}