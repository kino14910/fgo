using Fgo.Scripts.Cards.Colorless;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Events;

[RegisterSharedEvent]
public class ProofAndRebuttal : FgoEventModel
{
    private const int UpgradeCount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(0)
    ];

    public override void CalculateVars()
    {
        DynamicVars.Gold.BaseValue = Rng.NextInt(40, 61);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        // 检查牌组中是否有可升级的卡牌
        var hasUpgradableCards = Owner!.Deck.Cards.Any(c => c.IsUpgradable);

        if (hasUpgradableCards)
            return
            [
                new EventOption(this, Witness, InitialOptionKey("WITNESS")),
                new EventOption(this, Enjoy, InitialOptionKey("ENJOY"))
            ];

        return
        [
            new EventOption(this, Witness, InitialOptionKey("WITNESS")),
            new EventOption(this, null, InitialOptionKey("LOCKED"))
        ];
    }

    public async Task Witness()
    {
        var card = ModelDb.Card<Cards.Colorless.ProofAndRebuttal>().ToMutable();
        await CardPileCmd.Add(card, PileType.Hand);
        SetEventFinished(PageDescription("WITNESS"));
    }

    public async Task Enjoy()
    {
        await PlayerCmd.LoseGold(DynamicVars.Gold.IntValue, Owner!);

        // 随机升级牌组中的 2 张卡牌
        var upgradableCards = Owner!.Deck.Cards
            .Where(c => c.IsUpgradable)
            .OrderBy(_ => Random.Shared.Next())
            .Take(UpgradeCount)
            .ToList();

        foreach (var card in upgradableCards)
            if (card.IsUpgradable)
                CardCmd.Upgrade(card, CardPreviewStyle.None);

        SetEventFinished(PageDescription("ENJOY"));
    }
}