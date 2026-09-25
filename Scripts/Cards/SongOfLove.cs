using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     恋歌: 多人专用。指定一名盟友作为收星者，本回合内所有友方攻击产生的暴击星全部归其所有。
/// </summary>
public class SongOfLove() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<SongOfLovePower>(),
        FgoHoverTipFactory.FromNp(),
        FgoHoverTipFactory.FromStar()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Np", 30)
    ];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await PlayerCmd.GainEnergy(2, Owner);
        await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, Owner);

        // 同时只允许存在一个收星者：先清掉友方身上残留的旧能力，再给本次选中的盟友挂上。
        var allies = CombatState!.GetTeammatesOf(Owner.Creature).ToList();
        foreach (var ally in allies)
            if (ally.HasPower<SongOfLovePower>())
                await PowerCmd.Remove<SongOfLovePower>(ally);

        await PowerCmd.Apply<SongOfLovePower>(choiceContext, cardPlay.Target, 1m, Owner.Creature, this);
    }
}
