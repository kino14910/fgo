using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.Colorless.EventCards;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using RelicII = Fgo.Scripts.Relics.II;
using RelicAstralSwordEpitaph = Fgo.Scripts.Relics.AstralSwordEpitaph;

namespace Fgo.Scripts.Events;

[RegisterSharedEvent]
public sealed class FinalUnderworld : ModEventTemplate
{
    private const int QuartzCost = 3;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath:
        "res://Fgo/images/events/FinalUnderworld.png"
    );

    /// <summary>
    ///     仅当所有玩家都为 FGO 角色时生成（事件奖励依赖 FGO 专属的 NobleDeck 牌堆与圣晶石计数）。
    /// </summary>
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.Count > 0 && runState.Players.All(p => p.Character is FgoCharacter);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Accept, InitialOptionKey("ACCEPT")),
        new EventOption(this, Leave, InitialOptionKey("LEAVE"))
    ];

    /// <summary>接受她的招待: 获得女神的砂糖卡，进入下一页。</summary>
    private async Task Accept()
    {
        if (Owner == null) return;

        // 将女神的砂糖卡加入主卡组（非战斗牌堆用 RunState.CreateCard 创建实例）。
        var sugarCard = Owner.RunState.CreateCard(ModelDb.Card<GoddessSugar>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(sugarCard, Owner.Deck));

        ShowSugarPage();
    }

    private void ShowSugarPage()
    {
        SetEventState(PageDescription("SUGAR"), [
            new EventOption(this, Follow, ModOptionKey("SUGAR", "FOLLOW")),
            new EventOption(this, RefuseFollow, ModOptionKey("SUGAR", "REFUSE"))
        ]);
    }

    private async Task Follow()
    {
        if (Owner == null) return;

        await RelicCmd.Obtain<RelicII>(Owner);

        // 兽冠宝具加入 NobleDeck（RunPersistent），并播放加入预览动画。
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, Owner);
        if (noblePile != null)
        {
            var crown = Owner.RunState.CreateCard(ModelDb.Card<EdinShugurraCollapsar>(), Owner);
            var result = await CardPileCmd.Add(crown, noblePile);
            FgoCardActions.PreviewNoblePileAdd(result);
        }

        ShowFollowPage();
    }

    private void ShowFollowPage()
    {
        SetEventState(PageDescription("FOLLOW"), [
            new EventOption(this, Continue, ModOptionKey("FOLLOW", "CONTINUE")),
            new EventOption(this, Leave, ModOptionKey("FOLLOW", "LEAVE"))
        ]);
    }

    private Task Continue()
    {
        // 动态生成献祭选项: 圣晶石计数不足 3 层时不提供。
        var options = new List<EventOption>
        {
            new EventOption(this, RefuseGift, ModOptionKey("GIFT", "REFUSE"))
        };
        if (Owner != null && Entry.RunState.Get(Owner).QuartzCount >= QuartzCost)
            options.Insert(0, new EventOption(this, OfferQuartz, ModOptionKey("GIFT", "OFFER_QUARTZ")));

        SetEventState(PageDescription("GIFT"), options);
        return Task.CompletedTask;
    }

    /// <summary>献上 3 层圣晶石: 消耗计数，获得星剑的墓志铭。</summary>
    private async Task OfferQuartz()
    {
        if (Owner == null) return;

        Entry.RunState.Modify(Owner, data => data.QuartzCount -= QuartzCost);

        // 圣晶石/召唤券的计数显示同步刷新。
        Owner.GetRelic<Fgo.Scripts.Relics.SaintQuartz>()?.RefreshCounterVisual(QuartzCost);
        Owner.GetRelic<Fgo.Scripts.Relics.SummonTicket>()?.RefreshCounterVisual(QuartzCost);

        await RelicCmd.Obtain<RelicAstralSwordEpitaph>(Owner);

        SetEventFinished(PageDescription("GIFTED"));
    }

    private Task RefuseGift()
    {
        SetEventFinished(PageDescription("REFUSED"));
        return Task.CompletedTask;
    }

    private Task Leave()
    {
        SetEventFinished(PageDescription("LEFT"));
        return Task.CompletedTask;
    }

    private Task RefuseFollow()
    {
        SetEventFinished(PageDescription("LEFT"));
        return Task.CompletedTask;
    }
}