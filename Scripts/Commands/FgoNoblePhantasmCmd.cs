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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
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

        // 宝具升级次数（最多 MaxOverCharge 层）
        var overCharge = player.Creature.GetPower<OverchargePower>()?.Amount ?? 0;

        // 候选来自 NobleDeck pile（由 SaintQuartz 遗物管理初始卡 + 右键加入的卡）。
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null || noblePile.IsEmpty) return false;

        // pile 里的卡已经是注册到战斗的 mutable 实例，可直接用作候选。
        var cards = noblePile.Cards.OfType<NobleCardModel>().ToList();

        // NpCardPower: 角色拥有此 power 时，将对应的特定宝具卡加入候选列表。
        // NobleCard 存储的是 canonical singleton，可直接用作候选。
        var npCardPower = player.Creature.GetPower<NpCardPower>();
        if (npCardPower?.NobleCard is { } nobleCard
            && cards.All(c => c.Id != nobleCard.Id))
            cards.Add(nobleCard);

        if (cards.Count == 0) return false;

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "FGO_GAMEPLAY_UI_NP_TEXT.text_2"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, prefs)).FirstOrDefault();
        if (selected == null) return false;

        if (npCardPower != null)
            await PowerCmd.Remove(npCardPower);

        // NobleDeck 是 RunPersistent 牌堆，卡需跨战斗保留，因此不打出去原卡。
        // 而是从 canonical singleton 创建副本，按 OverCharge 升级后加入手牌。
        var canonical = ModelDb.GetByIdOrNull<NobleCardModel>(selected.Id);
        if (canonical == null)
            return false;

        var playCopy = (NobleCardModel)player.Creature.CombatState!.CreateCard(canonical, player);
        for (var i = 0; i < overCharge && playCopy.IsUpgradable; i++)
            CardCmd.Upgrade(playCopy, CardPreviewStyle.None);

        await CardPileCmd.AddGeneratedCardToCombat(playCopy, PileType.Hand, player);
        await playerState.SpendNpForNoblePhantasm();
        return true;
    }
}