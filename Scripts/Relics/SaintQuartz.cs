using Fgo.Scripts.Cards.DerivativeMash;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

/// <summary>
///     圣晶石: FGO 角色的初始遗物，仅负责圣晶石计数与右键消耗计数换取宝具卡。
///     - 每进入一个房间（每层）+1 计数。
///     - 计数 ≥ 3 时，可右键此遗物随机获得一张尚未在 NobleDeck 的宝具卡，并消耗 3 计数。
///     - 被 [gold]点金石[/gold] 祝福后升级为[gold]召唤券[/gold]（SummonTicket），可手动选择要加入的宝具卡。
///     - 计数存于按玩家的 FgoRunState（见 FgoRelic.QuartzCounter）: 升级替换遗物实例后计数保留。
///     - NobleDeck 牌堆的生命周期（播种初始宝具卡、按钮绑定）已与此遗物解耦，
///     改由 run 生命周期在 Entry 中统一处理（见 FgoCardActions.EnsureNobleDeckSeeded）。
/// </summary>
[RegisterCharacterStarterRelic(typeof(FgoCharacter))]
public class SaintQuartz : FgoRelic, IModRightClickableRelic
{
    private const int CostPerChoice = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;
    public override int DisplayAmount => QuartzCounter;

    /// <summary>
    ///     本地快速过滤: 只有计数 ≥ 3 时才接收右键。
    /// </summary>
    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return QuartzCounter >= CostPerChoice;
    }

    /// <summary>
    ///     执行期判定: 再次检查计数（防止本地与同步状态差异）。
    /// </summary>
    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        return QuartzCounter >= CostPerChoice;
    }

    /// <summary>
    ///     右键触发: 从 NobleCardPool 中尚未在 NobleDeck 的宝具卡中随机选一张加入 NobleDeck。
    /// </summary>
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (QuartzCounter < CostPerChoice) return;

        var player = context.Player;

        var existing = CardPile.Get(FgoEnums.NobleDeck, player)?.Cards
            .Select(c => c.GetType())
            .ToHashSet() ?? [];

        var excludeTypes = new HashSet<Type>
        {
            typeof(Camelot),
            typeof(LordCamelot),
            typeof(LordChaldeas),
            typeof(ObscurantWallOfChalk)
        };

        // 用 RunState.CreateCard 而非 CombatState.CreateCard，使右键在地图上也能使用。
        // NobleDeck 是 RunPersistent 牌堆，加入的卡不进入战斗 pile，无需注册到 CombatState。
        var candidates = ModelDb.CardPool<NobleCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .OfType<NobleCardModel>()
            .Where(card => !existing.Contains(card.GetType()) && !excludeTypes.Contains(card.GetType()))
            .Select(card => player.RunState.CreateCard(card, player))
            .ToList();

        if (candidates.Count == 0)
        {
            // 无可选项: 闪一下表示已确认但无候选，不消耗计数。
            Flash();
            return;
        }

        // 随机选择一张（使用 run RNG，保证种子可复现）。
        var selected = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (selected == null) return;

        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile != null)
        {
            // 用 CardPileCmd.Add 加卡以获得 CardPileAddResult，再触发卡牌飞入 NobleDeck 顶部栏牌组的特效。
            var result = await CardPileCmd.Add(selected, noblePile);
            QuartzCounter -= CostPerChoice;
            UpdateAvailableVisual(CostPerChoice);
            Flash();
            FgoCardActions.PreviewNoblePileAdd(result);
        }
    }

    /// <summary>
    ///     每进入一个房间（每层）+1。AfterRoomEntered 在战斗胜利后/事件触发后等都会调用，
    ///     对应"每层 +1"的语义。
    /// </summary>
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        QuartzCounter++;
        UpdateAvailableVisual(CostPerChoice);
        return Task.CompletedTask;
    }
}