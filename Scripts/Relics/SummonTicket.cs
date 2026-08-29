using Fgo.Scripts.Cards.DerivativeMash;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
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
    private const int CostPerChoice = 3;

    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool ShowCounter => true;
    public override int DisplayAmount => QuartzCounter;

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return QuartzCounter >= CostPerChoice;
    }

    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        return QuartzCounter >= CostPerChoice;
    }

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (QuartzCounter < CostPerChoice) return;
        var player = context.Player;
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
            QuartzCounter -= CostPerChoice;
            UpdateAvailableVisual(CostPerChoice);
            Flash();
            FgoCardActions.PreviewNoblePileAdd(result);
        }
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        QuartzCounter++;
        UpdateAvailableVisual(CostPerChoice);
        return Task.CompletedTask;
    }
}