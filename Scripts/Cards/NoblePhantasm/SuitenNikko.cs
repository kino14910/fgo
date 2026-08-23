using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class SuitenNikko() : NobleCardModel(2, CardType.Skill, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Np", 25),
        ModCardVars.Heal(6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(5);
        DynamicVars.Heal.UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<Creature> enumerable = from c in CombatState!.GetTeammatesOf(Owner.Creature)
            where c is { IsAlive: true, IsPlayer: true }
            select c;
        foreach (var item in enumerable)
        {
            if (item.Player is null) return;
            await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, item.Player);
            await CreatureCmd.Heal(item, DynamicVars.Heal.BaseValue);
            foreach (var card in PileType.Hand.GetPile(item.Player).Cards) card.EnergyCost.AddThisCombat(-1, true);
        }
    }
}