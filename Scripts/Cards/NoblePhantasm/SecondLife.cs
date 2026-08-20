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

        var card = exhaustCards[Random.Shared.Next(exhaustCards.Count)];
        // 用 CombatState.CreateCard 非泛型重载: 内部完成 ToMutable + 设 Owner + 注册 CombatState + AfterCreated。
        // 直接 ToMutable() 出来的副本无 Owner、未注册 CombatState，传入 AddToPile 会抛 InvalidOperationException。
        var copy = CombatState!.CreateCard(card, Owner);
        if (copy.IsUpgradable && IsUpgraded)
            CardCmd.Upgrade(copy, CardPreviewStyle.None);
        await FgoCardActions.AddToPile(copy, PileType.Hand);

        foreach (var enemy in CombatState!.HittableEnemies)
            if (enemy.HasPower<MinionPower>())
            {
                await CreatureCmd.Kill(enemy);
                await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, cardPlay.Player);
            }
    }
}