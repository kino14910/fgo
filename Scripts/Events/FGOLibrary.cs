using Fgo.Scripts.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Events;

[RegisterSharedEvent]
public class FGOLibrary : FgoEventModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new MaxHpVar(5m),
        new HealVar(15m)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            new EventOption(this, Read, InitialOptionKey("READ")),
            new EventOption(this, Sleep, InitialOptionKey("SLEEP")),
            new EventOption(this, Leave, InitialOptionKey("LEAVE"))
        ];
    }

    public async Task Read()
    {
        // 失去 Max HP 后从角色卡池中选择一张卡牌加入牌组
        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner!.Creature, DynamicVars.MaxHp.IntValue,
            true);

        var pool = ModelDb.CardPool<FgoCardPool>();
        var availableCards = pool.GetUnlockedCards(Owner!.UnlockState, Owner!.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity != CardRarity.Basic)
            .ToList();

        if (availableCards.Count > 0)
        {
            var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "FGOLibrary.text_read"), 1)
            {
                RequireManualConfirmation = true
            };
            var selected = (await CardSelectCmd.FromSimpleGrid(
                new BlockingPlayerChoiceContext(), availableCards, Owner!, prefs)).FirstOrDefault();

            if (selected != null)
            {
                var card = selected.ToMutable();
                await CardPileCmd.Add(card, Owner!.Deck);
            }
        }

        SetEventFinished(PageDescription("READ"));
    }

    public async Task Sleep()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.BaseValue);
        SetEventFinished(PageDescription("SLEEP"));
    }

    public async Task Leave()
    {
        SetEventFinished(PageDescription("LEAVE"));
    }
}