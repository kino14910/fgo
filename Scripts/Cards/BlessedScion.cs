using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class BlessedScion() : FgoCardModel(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Copies", 1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Copies"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature.Player;
        var selectable = player!.PlayerCombatState!.Hand.Cards.ToList();
        if (selectable.Count == 0) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0,
            Math.Min(DynamicVars["Copies"].IntValue, selectable.Count));
        var selected = await CardSelectCmd.FromHand(choiceContext, player, prefs, _ => true, this);
        await FgoCardActions.AddCopiesToHand(selected, true);
    }
}