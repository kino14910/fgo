using System;
using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Networking.ManagedActions;

namespace Fgo.Scripts.Commands;

public static class FgoMoonRabbitFormCmd
{
    // 载荷只带手牌 index + 目标形态，不引用卡牌模型身份；各端在动作重放里按同一 index 定位。
    internal static readonly RitsuLibManagedNetActionDescriptor<int> SyncDescriptor = new(
        Entry.ModId,
        "moon_rabbit_form_v1",
        static payload => BitConverter.GetBytes(payload),
        static bytes => bytes.Length >= 4 ? BitConverter.ToInt32(bytes) : 0,
        ExecuteManaged,
        GameActionType.CombatPlayPhaseOnly);

    public static bool Request(int handIndex, bool targetNap)
    {
        if (CombatManager.Instance.IsInProgress &&
            RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
            return false;

        var payload = (handIndex << 1) | (targetNap ? 1 : 0);
        return RitsuLibManagedNetActions.Request<int>(RunManager.Instance, SyncDescriptor, payload);
    }

    private static async Task ExecuteManaged(RitsuLibManagedNetActionContext<int> context)
    {
        if (CombatManager.Instance.IsEnding) return;

        var hand = PileType.Hand.GetPile(context.Player);
        var index = context.Message >> 1;
        if (index < 0 || index >= hand.Cards.Count) return;

        var card = hand.Cards[index];
        if (card is not MoonRabbit && card is not MoonRabbitNap) return;

        var targetNap = (context.Message & 1) != 0;
        if ((card is MoonRabbitNap) == targetNap) return;

        if (card.IsUpgraded)
        {
            CardModel replacement = targetNap
                ? card.CardScope!.CreateCard<MoonRabbitNap>(card.Owner)
                : card.CardScope!.CreateCard<MoonRabbit>(card.Owner);
            CardCmd.Upgrade(replacement, CardPreviewStyle.None);
            await CardCmd.Transform(card, replacement, CardPreviewStyle.None);
        }
        else if (targetNap)
        {
            await CardCmd.TransformTo<MoonRabbitNap>(card, CardPreviewStyle.None);
        }
        else
        {
            await CardCmd.TransformTo<MoonRabbit>(card, CardPreviewStyle.None);
        }
    }
}

public sealed class FgoMoonRabbitRightClickHandler : IModRightClickHandler
{
    public int Priority => 0;

    public bool TryHandle(ModRightClickContext context)
    {
        if (context.Model is not CardModel card) return false;
        if (card is not MoonRabbit && card is not MoonRabbitNap) return false;
        if (card.Pile?.Type != PileType.Hand) return false;
        if (card.Owner.NetId != context.Player.NetId) return false;
        if (card.Owner.Creature.CombatState == null) return false;

        var hand = PileType.Hand.GetPile(context.Player);
        var index = -1;
        for (var i = 0; i < hand.Cards.Count; i++)
        {
            if (!ReferenceEquals(hand.Cards[i], card)) continue;
            index = i;
            break;
        }
        if (index < 0) return false;

        return FgoMoonRabbitFormCmd.Request(index, card is MoonRabbit);
    }
}
