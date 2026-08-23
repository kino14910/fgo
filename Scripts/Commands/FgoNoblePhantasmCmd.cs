using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Fgo.Scripts.Commands;

public static class FgoNoblePhantasmCmd
{
    public static async Task<bool> TryChooseNoblePhantasm(PlayerChoiceContext choiceContext, Player player)
    {
        var playerState = FgoBattleHooks.Get(player);
        if (!playerState.CanUseNp)
            return false;

        if (player.Creature.HasPower<SealNpPower>())
            return false;

        // 宝具升级次数 = 当前 OverchargePower 的层数（最多 MaxOverCharge 层）。
        var overCharge = player.Creature.GetPower<OverchargePower>()?.Amount ?? 0;

        // 候选来自 NobleDeck pile（由 SaintQuartz 遗物管理初始卡 + 右键加入的卡）。
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null || noblePile.IsEmpty) return false;

        // pile 里的卡已经是注册到战斗的 mutable 实例，可直接用作候选。
        var cards = noblePile.Cards.OfType<NobleCardModel>().ToList();

        // NpCardPower: 角色拥有此 power 时，将对应的特定宝具卡加入候选列表。
        // NobleCard 存储的是 canonical singleton，可直接用作候选。
        // 选择宝具牌后 NpCardPower 会被移除，之后无法再选择这些卡。
        var npCardPower = player.Creature.GetPower<NpCardPower>();
        if (npCardPower?.NobleCard is { } nobleCard
            && cards.All(c => c.Id != nobleCard.Id))
            cards.Add(nobleCard);

        if (cards.Count == 0) return false;

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "FGO_GAMEPLAY_UI_NP_TEXT.text_2"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, prefs)).FirstOrDefault();
        if (selected == null) return false;

        // 选择宝具牌后，移除 NpCardPower（无论选了哪张牌）。
        // 移除后，下次选择宝具牌时不再有 NpCardPower 添加的特定卡。
        if (npCardPower != null)
            await PowerCmd.Remove(npCardPower);

        // NobleDeck 是 RunPersistent 牌堆，卡需跨战斗保留，因此不打出去原卡。
        // 改为从 canonical singleton 创建副本，按 OverCharge 升级后加入手牌。
        // 玩家手动打出副本；NobleCardModel.OnPlay 末尾会调用 CardPileCmd.RemoveFromCombat，
        // 让副本从本次战斗消失（不进弃牌堆/消耗堆），原卡仍留在 NobleDeck。
        // 通过 ModelId 获取 canonical singleton，避免反射。
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