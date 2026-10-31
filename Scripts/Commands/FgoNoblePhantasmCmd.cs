using System.Text;
using System.Text.Json;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.ManagedActions;

namespace Fgo.Scripts.Commands;

public static class FgoNoblePhantasmCmd
{
    /// <summary>
    ///     托管网络动作: 把宝具选牌作为正式 GameAction 走官方动作队列（药水 UsePotionAction 模式），
    ///     在所有 peer 上执行。选牌用 GameActionPlayerChoiceContext pause/resume 动作本身，
    ///     选择会正确路由给动作所有者。此前直接在 UI 事件里跑选牌导致 host 侧无人执行
    ///     SignalPlayerChoiceBegun，hook action 永远等不到 SetChoiceContext，队列死锁卡死。
    ///     必须在 Entry.Init 注册，保证任何 peer 发起前本端已注册（ExecuteAction 按 opcode 查找）。
    /// </summary>
    internal static readonly RitsuLibManagedNetActionDescriptor<string> SyncDescriptor = new(
        Entry.ModId,
        "np_button",
        static ids => Encoding.UTF8.GetBytes(ids),
        static bytes => Encoding.UTF8.GetString(bytes),
        ExecuteManaged,
        GameActionType.CombatPlayPhaseOnly);

    private static async Task ExecuteManaged(RitsuLibManagedNetActionContext<string> context)
    {
        // 候选由发起端随 payload 下发，各端不再各自读本地 NobleDeck。
        var candidates = DeserializeCandidates(context.Message);
        await TryChooseNoblePhantasm(context.PlayerChoiceContext, context.Player, candidates);
    }

    /// <summary>
    ///     UI 按钮入口: 仅本机玩家调用（调用方需保证 LocalContext.IsMe）。
    ///     候选列表在本机（拥有者）定稿后随动作 payload 下发 —— 选牌结果按 index 同步，
    ///     只要各端候选相同，任何 NobleDeck 副本差异都不会再解析出不同的宝具。
    /// </summary>
    public static bool Request(Player player)
    {
        if (CombatManager.Instance.IsInProgress &&
            RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
            return false;

        var cards = BuildCandidates(player);
        if (cards.Count == 0) return false;

        Entry.Logger.Info(
            $"[Fgo] np_button candidates for netId={player.NetId}: [{string.Join(", ", cards.Select(c => c.Id.Entry))}]");

        var payload = JsonSerializer.Serialize(cards.Select(c => c.Id.ToString()).ToList());
        return RitsuLibManagedNetActions.Request<string>(RunManager.Instance, SyncDescriptor, payload);
    }

    /// <summary>
    ///     发起端生成候选：NobleDeck 现有宝具 + NpCardPower 追加的宝具。
    ///     顺序即 payload 顺序，也是各端选牌界面的顺序。
    /// </summary>
    private static List<NobleCardModel> BuildCandidates(Player player)
    {
        // 候选来自 NobleDeck pile（由 SaintQuartz 遗物管理初始卡 + 右键加入的卡）。
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null || noblePile.IsEmpty)
        {
            Entry.Logger.Warn(
                $"[Fgo] np_button skipped for netId={player.NetId}: NobleDeck {(noblePile == null ? "missing" : "empty")} on this peer!");
            return [];
        }

        // pile 里的卡是注册到战斗的 mutable 实例；其顺序即选牌界面的候选顺序。
        var cards = noblePile.Cards.OfType<NobleCardModel>().ToList();

        // NpCardPower: 角色拥有此 power 时，将对应的特定宝具卡加入候选列表。
        // 该追加候选会一直保留 —— 只有玩家真正选中它时 power 才被移除，
        // 因此中途选了别的宝具不会白白丢掉这份候选。
        // NobleCard 存储的是 canonical singleton，可直接用作候选。
        var npCardPower = player.Creature.GetPower<NpCardPower>();
        if (npCardPower?.NobleCard is { } nobleCard
            && cards.All(c => c.Id != nobleCard.Id))
            cards.Add(nobleCard);

        if (cards.Count == 0)
        {
            Entry.Logger.Warn(
                $"[Fgo] np_button skipped for netId={player.NetId}: NobleDeck has {noblePile.Cards.Count} card(s) but no noble card.");
        }

        return cards;
    }

    /// <summary>
    ///     各端按 payload 重建候选：严格按发起端下发的 Id 顺序，不读本地 NobleDeck，
    ///     因此各端候选数量与顺序必然一致（index 同步的选牌结果才能解析到同一张卡）。
    ///     任一 Id 解析失败就整段放弃 —— 各端收到的是同一 payload，放弃同样是确定性的。
    /// </summary>
    private static List<NobleCardModel> DeserializeCandidates(string payload)
    {
        try
        {
            var ids = JsonSerializer.Deserialize<List<string>>(payload) ?? [];
            var cards = new List<NobleCardModel>(ids.Count);
            foreach (var idText in ids)
            {
                var id = ModelId.Deserialize(idText);
                var canonical = ModelDb.GetByIdOrNull<NobleCardModel>(id);
                if (canonical == null)
                {
                    Entry.Logger.Warn(
                        $"[Fgo] np_button skipped: payload contains unknown noble card '{idText}'.");
                    return [];
                }

                cards.Add(canonical);
            }

            return cards;
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Fgo] np_button skipped: malformed candidate payload: {ex.Message}");
            return [];
        }
    }

    public static async Task<bool> TryChooseNoblePhantasm(PlayerChoiceContext choiceContext, Player player,
        IReadOnlyList<NobleCardModel> candidates)
    {
        var playerState = FgoBattleHooks.Get(player);
        if (!playerState.CanUseNp)
        {
            Entry.Logger.Warn($"[Fgo] np_button skipped for netId={player.NetId}: NP={playerState.Np} < 100.");
            return false;
        }

        if (player.Creature.HasPower<SealNpPower>())
        {
            Entry.Logger.Warn($"[Fgo] np_button skipped for netId={player.NetId}: SealNpPower present.");
            return false;
        }

        if (candidates.Count == 0)
        {
            Entry.Logger.Warn($"[Fgo] np_button skipped for netId={player.NetId}: no candidates in payload.");
            return false;
        }

        var overCharge = player.Creature.GetPower<OverchargePower>()?.Amount ?? 0;

        // 候选顺序来自动作 payload（发起端 NobleDeck 的顺序），各端必须严格按它渲染与解析 index。
        var cards = candidates.ToList();

        // 选宝具页按当前 OC 预览强化后的数值：候选换成「已按 OC 逐级强化」的展示副本。
        // 副本只用于渲染与选择（从 canonical 拷贝、不注册进战斗、不属于任何牌堆），
        // 选完仍按 Id 取 canonical 生成真正的战斗卡，因此不改变既有行为。
        // 顺序与索引和 cards 完全一致 —— 联机选牌结果按 index 同步，位置不能变。
        var previews = cards.Select(card => BuildPreview(card, player, overCharge) ?? card).ToList();

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "FGO_GAMEPLAY_UI_NP_TEXT.text_2"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, previews, player, prefs)).FirstOrDefault();
        if (selected == null) return false;

        // selected 可能是展示副本（BuildPreview），其 Id 与 canonical 一致，故按 Id 而非引用比对。
        // NobleCard 未赋值时照旧移除，避免 power 永久残留在角色身上。
        var npCardPower = player.Creature.GetPower<NpCardPower>();
        var grantedCard = npCardPower?.NobleCard;
        if (npCardPower != null && (grantedCard == null || selected.Id == grantedCard.Id))
            await PowerCmd.Remove(npCardPower);

        // NobleDeck 是 RunPersistent 牌堆，卡需跨战斗保留，因此不打出去原卡。
        // 而是从 canonical singleton 创建副本，按 Overcharge 升级后加入手牌。
        var canonical = ModelDb.GetByIdOrNull<NobleCardModel>(selected.Id);
        if (canonical == null)
            return false;

        var playCopy = (NobleCardModel)player.Creature.CombatState!.CreateCard(canonical, player);

        // OC 每层强化一次：按 OC 层数逐级升级副本（NobleCardModel.MaxUpgradeLevel = MaxOvercharge，
        // 因此 overCharge 层数可完整生效；此前依赖单级 IsUpgradable 会在首次强化后提前终止）。
        FgoCardActions.ApplyUpgradeLevels(playCopy, overCharge);

        await CardPileCmd.AddGeneratedCardToCombat(playCopy, PileType.Hand, player);
        await playerState.SpendNpForNoblePhantasm();
        return true;
    }

    /// <summary>
    ///     生成选宝具页的展示副本：从 canonical 拷贝一个可变实例，按 OC 层数逐级强化，
    ///     使候选卡在选牌界面直接显示「获得 OC 强化后」的数值（标题同步显示 卡名+N）。
    ///     副本不注册进战斗、不属于任何牌堆，纯展示用；canonical 缺失时返回 null，由调用方回退原卡。
    /// </summary>
    private static NobleCardModel? BuildPreview(NobleCardModel card, Player player, int upgradeLevels)
    {
        if (ModelDb.GetByIdOrNull<NobleCardModel>(card.Id) is not { } canonical) return null;

        var preview = (NobleCardModel)canonical.ToMutable();
        preview.GiveToAnotherPlayer(player);
        FgoCardActions.ApplyUpgradeLevels(preview, upgradeLevels);
        return preview;
    }
}