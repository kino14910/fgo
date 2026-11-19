using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class SecondLife() : NobleCardModel(1, CardType.Skill, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipFactory.FromNp()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Np", 20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cardModel = (await CardSelectCmd.FromCombatPile(choiceContext,
            PileType.Exhaust.GetPile(Owner), Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1)
        )).FirstOrDefault();
        if (cardModel != null) await CardPileCmd.Add(cardModel, PileType.Hand);

        await KillMinions(Owner, DynamicVars["Np"].IntValue, cardPlay.Card);
    }

    private static async Task KillMinions(Player player, int npPerMinion, CardModel source)
    {
        // Player.Character 是共享单例，判归属一律走 Creature / CombatState以保持同步。
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return;

        var minions = combatState.Enemies
            .Where(static e => e is { IsAlive: true, IsSecondaryEnemy: true })
            .ToList();

        if (minions.Count == 0)
            return;

        await CreatureCmd.Kill(minions, source);
        await FgoResCmd.ModifyNp(npPerMinion * minions.Count, player);
    }
}