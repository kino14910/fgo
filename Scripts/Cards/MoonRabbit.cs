using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

/// <summary>
///     月下逍遥 / 悠然午睡 —— 通过【右键】切换两种形态，卡名、卡图与效果随之改变。
///     月下逍遥：获得 30% [gold]暴击威力[/gold]，回复 5(8) 点生命。
///     悠然午睡：获得 20(30)% [gold]宝具威力[/gold]，获得 1 层缓冲（阻止下一次生命值损伤）。
/// </summary>
public class MoonRabbit() : FgoCardModel(2, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self), IModRightClickableCard
{
    private const string MoonPortrait = "res://Fgo/images/cards/DancingUnderTheFullMoon.png";
    private const string NapPortrait = "res://Fgo/images/cards/NappingUnderTheSun.png";
    private const string SkillFrame = "res://Fgo/images/card_frames/card_frame_skill.png";

    private const string MoonTitleKey = "FGO_CARD_MOON_RABBIT.title";
    private const string NapTitleKey = "FGO_CARD_MOON_RABBIT.title_nap";

    private const int UpgradeHealBonus = 3;
    private const int UpgradeNpDamageBonus = 10;

    /// <summary>当前形态：false = 月下逍遥，true = 悠然午睡。随卡牌存档持久化。</summary>
    [SavedProperty]
    public bool NapMode { get; set; }

    public override string Title
    {
        get
        {
            var text = new LocString("cards", NapMode ? NapTitleKey : MoonTitleKey).GetFormattedText();
            if (!IsUpgraded) return text;
            return MaxUpgradeLevel > 1 ? $"{text}+{CurrentUpgradeLevel}" : text + "+";
        }
    }

    public override CardAssetProfile AssetProfile =>
        new(NapMode ? NapPortrait : MoonPortrait, null, SkillFrame);

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalDamagePower>(),
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<BufferPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<CriticalDamagePower>(30),
        ModCardVars.Heal(5),
        ModCardVars.Power<NpDamagePower>(20),
        ModCardVars.Int("ShowMoon", 1),
        ModCardVars.Int("ShowNap", 1),
        ModCardVars.Int("ShowBoth", 1),
        ModCardVars.Int("ShowHint", 0)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(UpgradeHealBonus);
        DynamicVars[nameof(NpDamagePower)].UpgradeValueBy(UpgradeNpDamageBonus);
        UpdateModeDisplay();
    }

    protected override void AfterDowngraded()
    {
        UpdateModeDisplay();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        if (NapMode)
        {
            await PowerCmd.Apply<NpDamagePower>(choiceContext, creature,
                DynamicVars[nameof(NpDamagePower)].BaseValue, creature, this);
            await PowerCmd.Apply<BufferPower>(choiceContext, creature, 1m, creature, this);
            return;
        }

        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, creature,
            DynamicVars[nameof(CriticalDamagePower)].BaseValue, creature, this);
        await CreatureCmd.Heal(creature, DynamicVars["Heal"].BaseValue);
    }

    public Task OnRightClick(ModRightClickExecutionContext context)
    {
        var next = !NapMode;
        NapMode = next;

        // 战斗内是克隆体，形态必须同步写回主卡组本体，否则战斗结束后丢失。
        if (DeckVersion is MoonRabbit master && !ReferenceEquals(master, this))
            master.NapMode = next;

        UpdateModeDisplay();
        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this) UpdateModeDisplay();
        return Task.CompletedTask;
    }

    private void UpdateModeDisplay()
    {
        if (DynamicVars == null) return;

        // 对局内只显示当前形态；奖励/图鉴等非对局环境两形态并列。
        var inCombat = Pile != null;
        var showMoon = !inCombat || !NapMode;
        var showNap = !inCombat || NapMode;

        DynamicVars["ShowMoon"].BaseValue = showMoon ? 1 : 0;
        DynamicVars["ShowNap"].BaseValue = showNap ? 1 : 0;
        DynamicVars["ShowBoth"].BaseValue = showMoon && showNap ? 1 : 0;
        DynamicVars["ShowHint"].BaseValue = inCombat ? 1 : 0;

        // UpdateVisuals 会重读 Title、PortraitPath 与 description，卡名/卡图/文案一起刷新。
        var node = NCard.FindOnTable(this);
        if (node != null)
            node.UpdateVisuals(Pile?.Type ?? PileType.Hand, CardPreviewMode.Normal);
    }
}
