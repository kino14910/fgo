using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class SecondLife() : NobleCardModel(1, CardType.Skill, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateNpHoverTip()
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
        var exhaustCards = Owner.PlayerCombatState!.ExhaustPile.Cards.ToList();
        if (exhaustCards.Count == 0) return;

        var card = Owner.RunState.Rng.CombatCardSelection.NextItem(exhaustCards);
        if (card is null) return;
        var copy = card.CreateClone();
        if (copy.IsUpgradable && IsUpgraded)
            CardCmd.Upgrade(copy, CardPreviewStyle.None);
        await FgoCardActions.AddToHand(copy);

        foreach (var enemy in CombatState!.HittableEnemies.Where(enemy => enemy.HasPower<MinionPower>()).ToList())
        {
            await CreatureCmd.Kill(enemy);
            await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, cardPlay.Player);
        }
    }
}