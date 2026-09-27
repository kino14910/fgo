using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Cards;

/// <summary>
///     〔纳刀〕: 下一次选宝具时把〔拔刀·神威〕追加进候选，并在其打出前封锁其他攻击牌。
///     追加候选沿用光之地平线的 NpCardPower（选完宝具即消耗）；出牌封锁由
///     <see cref="SwordSheathingPower" /> 负责，直到拔刀·神威真正打出才解除。
/// </summary>
public class SwordSheathing() : FgoCardModel(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpCardPower>(),
        HoverTipFactory.FromPower<SwordSheathingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var npCardPower = await PowerCmd.Apply<NpCardPower>(choiceContext, Owner.Creature, 1m,
            Owner.Creature, this);
        if (npCardPower != null)
            npCardPower.NobleCard = ModelDb.Card<BattouKamui>();

        await PowerCmd.Apply<SwordSheathingPower>(choiceContext, Owner.Creature, 1m,
            Owner.Creature, this);
    }
}
