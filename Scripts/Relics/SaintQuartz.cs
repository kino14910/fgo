using Fgo.Scripts.Cards.DerivativeMash;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

/// <summary>
///     圣晶石: FGO 角色的初始遗物，管理宝具卡堆（NobleDeck）。
///     - 每进入一个房间（每层）+1 计数。
///     - 计数 ≥ 3 时，可右键此遗物随机获得一张尚未在 NobleDeck 的宝具卡，并消耗 3 计数。
///     - 被 [gold]点金石[/gold] 祝福后升级为[gold]召唤券[/gold]（SummonTicket），可手动选择要加入的宝具卡。
///     - NobleDeck 为 RunPersistent 牌堆: 在获得遗物时（run 开始）初始化初始宝具卡，
///     跨战斗保留、随存档保存。退出战斗不会清空。
/// </summary>
[RegisterCharacterStarterRelic(typeof(FgoCharacter))]
public class SaintQuartz : FgoRelic, IModRightClickableRelic
{
    private const int CostPerChoice = 3;
    private int _counter;
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;
    public override int DisplayAmount => _counter;

    /// <summary>
    ///     本地快速过滤: 只有计数 ≥ 3 时才接收右键。
    /// </summary>
    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return _counter >= CostPerChoice;
    }

    /// <summary>
    ///     执行期判定: 再次检查计数（防止本地与同步状态差异）。
    /// </summary>
    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        return _counter >= CostPerChoice;
    }

    /// <summary>
    ///     右键触发: 从 NobleCardPool 中尚未在 NobleDeck 的宝具卡中随机选一张加入 NobleDeck。
    /// </summary>
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (_counter < CostPerChoice) return;

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
            // 用 CardPileCmd.Add 加卡以获得 CardPileAddResult，再触发卡牌进入牌堆的预览特效。
            var result = await CardPileCmd.Add(selected, noblePile);
            _counter -= CostPerChoice;
            InvokeDisplayAmountChanged();
            Flash();
            CardCmd.PreviewCardPileAdd(result, 2.2f);
        }
    }

    /// <summary>
    ///     每进入一个房间（每层）+1。AfterRoomEntered 在战斗胜利后/事件触发后等都会调用，
    ///     对应"每层 +1"的语义。
    /// </summary>
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        _counter++;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     获得遗物时初始化 NobleDeck。NobleDeck 为 RunPersistent 牌堆，
    ///     在新 run 开始时（starter relic 获得）加入 BeautifulJourney、Camelot 各一张。
    ///     若已有卡（存档恢复后），不重复添加。
    /// </summary>
    public override async Task AfterObtained()
    {
        var player = Owner;

        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null) return;

        // 存档恢复后 pile 已有卡，不重复添加。
        if (noblePile.Cards.Count > 0)
        {
            RebindNobleDeckButton(player);
            return;
        }

        // 非战斗环境用 RunState.CreateCard(canonicalTemplate, owner) 创建 mutable 实例。
        var journey = player.RunState.CreateCard(ModelDb.Card<BeautifulJourney>(), player);
        var camelot = player.RunState.CreateCard(ModelDb.Card<Camelot>(), player);
        noblePile.AddInternal(journey);
        noblePile.AddInternal(camelot);

        RebindNobleDeckButton(player);
        await Task.CompletedTask;
    }

    /// <summary>
    ///     战斗开始时重新绑定 NobleDeck 按钮。
    ///     RunPersistent 牌堆本身跨战斗保留，但顶栏按钮需要在战斗开始时重新绑定
    ///     到当前 PlayerCombatState 上下文，以正确响应点击和刷新计数。
    /// </summary>
    public override Task BeforeCombatStart()
    {
        RebindNobleDeckButton(Owner);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     在顶栏中找到 NobleDeck 按钮并重新初始化，使其绑定到当前 player。
    /// </summary>
    private static void RebindNobleDeckButton(Player player)
    {
        var globalUi = NRun.Instance?.GlobalUi;
        var button = globalUi?.FindChild("ModCardPileButton_FGO_CARDPILE_NOBLE", true, false) as NModCardPileButton;
        button?.Initialize(player);
    }
}