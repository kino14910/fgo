using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
        // 卡面只承诺「升级」一次（{IfUpgraded:show:升级并且|}），升级层数不随本卡的 OC 层数放大。
        // 注意 CreateClone 已继承源卡的升级层数，这里是在其之上再额外升一级，并受该卡 MaxUpgradeLevel 限制。
        if (IsUpgraded)
            FgoCardActions.ApplyUpgradeLevels(copy, 1);
        await FgoCardActions.AddToHand(copy);

        FgoKillMinionsCmd.Request(cardPlay.Player, (int)DynamicVars["Np"].BaseValue);
    }
}
