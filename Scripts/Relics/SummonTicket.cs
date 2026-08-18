using Fgo.Scripts.Cards.DerivativeMash;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

/// <summary>
///     召唤券: 圣晶石被 [gold]点金石[/gold] 祝福后的升级版。
///     保留了每层 +1 计数的被动，但右键弹出候选网格后可手动选择加入哪张宝具卡。
/// </summary>
[RegisterRelic(typeof(FgoRelicPool))]
public class SummonTicket : FgoRelic, IModRightClickableRelic
{
    private const int CostPerChoice = 3;
    private int _counter;
    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool ShowCounter => true;
    public override int DisplayAmount => _counter;

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return _counter >= CostPerChoice;
    }

    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        return _counter >= CostPerChoice;
    }

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (_counter < CostPerChoice) return;

        var player = context.Player;
        var prefs = new CardSelectorPrefs(
            new LocString("gameplay_ui", "FGO_GAMEPLAY_UI_SUMMON_TICKET.text"),
            1)
        {
            RequireManualConfirmation = true
        };

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

        var candidates = ModelDb.CardPool<NobleCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .OfType<NobleCardModel>()
            .Where(card => !existing.Contains(card.GetType()) && !excludeTypes.Contains(card.GetType()))
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
            _counter -= CostPerChoice;
            InvokeDisplayAmountChanged();
            Flash();
            CardCmd.PreviewCardPileAdd(result, 2.2f);
        }
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        _counter++;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task AfterObtained()
    {
        var player = Owner;

        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null) return;

        RebindNobleDeckButton(player);
        await Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        RebindNobleDeckButton(Owner);
        return Task.CompletedTask;
    }

    private static void RebindNobleDeckButton(Player player)
    {
        var globalUi = NRun.Instance?.GlobalUi;
        var button = globalUi?.FindChild("ModCardPileButton_FGO_CARDPILE_NOBLE", true, false) as NModCardPileButton;
        button?.Initialize(player);
    }
}