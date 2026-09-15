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
    internal static readonly RitsuLibManagedNetActionDescriptor<byte> SyncDescriptor = new(
        Entry.ModId,
        "np_button",
        static _ => [],
        static _ => 0,
        ExecuteManaged,
        GameActionType.CombatPlayPhaseOnly);

    private static async Task ExecuteManaged(RitsuLibManagedNetActionContext<byte> context)
    {
        await TryChooseNoblePhantasm(context.PlayerChoiceContext, context.Player);
    }

    /// <summary>
    ///     UI 按钮入口: 仅本机玩家调用（调用方需保证 LocalContext.IsMe）。
    /// </summary>
    public static bool Request()
    {
        if (CombatManager.Instance.IsInProgress &&
            RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
            return false;

        return RitsuLibManagedNetActions.Request<byte>(RunManager.Instance, SyncDescriptor, 0);
    }

    public static async Task<bool> TryChooseNoblePhantasm(PlayerChoiceContext choiceContext, Player player)
    {
        var playerState = FgoBattleHooks.Get(player);
        if (!playerState.CanUseNp)
            return false;

        if (player.Creature.HasPower<SealNpPower>())
            return false;

        // OC 层数 = 宝具强化次数（0 ~ OverchargePower.MaxOvercharge，上限由 FgoBattleHooks 的
        // TryModifyPowerAmountReceived 保证，故此处读到的 Amount 必定在范围内）。
        var overCharge = player.Creature.GetPower<OverchargePower>()?.Amount ?? 0;

        // 候选来自 NobleDeck pile（由 SaintQuartz 遗物管理初始卡 + 右键加入的卡）。
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null || noblePile.IsEmpty) return false;

        // pile 里的卡是注册到战斗的 mutable 实例；其顺序即选牌界面的候选顺序。
        var cards = noblePile.Cards.OfType<NobleCardModel>().ToList();

        // NpCardPower: 角色拥有此 power 时，将对应的特定宝具卡加入候选列表。
        // NobleCard 存储的是 canonical singleton，可直接用作候选。
        var npCardPower = player.Creature.GetPower<NpCardPower>();
        if (npCardPower?.NobleCard is { } nobleCard
            && cards.All(c => c.Id != nobleCard.Id))
            cards.Add(nobleCard);

        if (cards.Count == 0) return false;

        // 选宝具页按当前 OC 预览强化后的数值：候选换成「已按 OC 逐级强化」的展示副本。
        // 副本只用于渲染与选择（从 canonical 拷贝、不注册进战斗、不属于任何牌堆），
        // 选完仍按 Id 取 canonical 生成真正的战斗卡，因此不改变既有行为。
        // 顺序与索引和 cards 完全一致 —— 联机选牌结果按 index 同步，位置不能变。
        var previews = cards.Select(card => BuildPreview(card, player, overCharge) ?? card).ToList();

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "FGO_GAMEPLAY_UI_NP_TEXT.text_2"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, previews, player, prefs)).FirstOrDefault();
        if (selected == null) return false;

        if (npCardPower != null)
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