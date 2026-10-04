using Fgo.Scripts.Cards.Colorless.Weapons;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     NFF 特别服务: 将手牌中若干张牌转换为〔军火经销商〕。
/// </summary>
/// <remarks>
///     选牌用 <c>CardSelectCmd.FromHand</c>（候选就是本端手牌，各端一致；结果按 index 跨端同步），
///     转换用 <c>CardCmd.TransformTo</c> 原地替换，保留原牌的位置。
/// </remarks>
public class NffSpecial() : FgoCardModel(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<GunDealer>()
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
        // 按"恰好 count 张"选: 手牌不足时把数量压到实际手牌数，否则选牌界面开不出来。
        var count = Math.Min(DynamicVars.Cards.IntValue, PileType.Hand.GetPile(Owner).Cards.Count);
        if (count <= 0) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, count);
        var selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).ToArray();

        foreach (var card in selected)
            await CardCmd.TransformTo<GunDealer>(card);
    }
}
