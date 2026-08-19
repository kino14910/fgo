using Fgo.Scripts.Cards.Colorless;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class NffSpecial() : FgoCardModel(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<PoisonousDagger>(),
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        for (var i = 0; i < 2; i++)
        {
            // 战斗中加卡必须用 CombatState.CreateCard<T>: 它完成 ToMutable + 设置 Owner + 注册到 CombatState。
            // ModelDb.Card<T>() 是全局规范单例（Owner 为 null），直接传入会抛 InvalidOperationException 并污染单例。
            var dagger = CombatState!.CreateCard<PoisonousDagger>(Owner);
            await FgoCardActions.AddToPile(dagger, PileType.Discard);
        }
    }
}