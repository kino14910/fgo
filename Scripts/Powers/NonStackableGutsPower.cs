using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     不叠加的毅力基类: 每次施加独立存在（各占一个图标），死亡时消耗一次次数并按回血量恢复。
///     每个来源（卡牌）对应一个具体子类，仿照 TemporaryStrengthPower 的写法：
///     不同来源是不同 Power 类型，天然互不合并；同来源重复施加也各自独立（InstanceType.Instanced）。
///     剩余次数 Times 归零后不再阻止死亡。
/// </summary>
public abstract class NonStackableGutsPower : GutsPower, IPowerExtraIconAmountLabelSpecsProvider
{
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     每次施加都新建独立实例，重复打出同来源卡也各自计次数。
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/GutsPower.png",
        "res://Fgo/images/powers/big/GutsPower.png"
    );

    /// <summary>
    ///     施加此毅力的来源模型（如打出它的卡牌），用于标题与悬浮提示。
    /// </summary>
    public abstract AbstractModel OriginModel { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("times", 1)
    ];

    public decimal Times
    {
        get => DynamicVars["times"].BaseValue;
        set
        {
            DynamicVars["times"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    /// <summary>
    ///     标题显示来源卡牌名（与 TemporaryStrengthPower 一致）。
    /// </summary>
    public override LocString Title
    {
        get
        {
            if (OriginModel is CardModel card)
                return card.TitleLocString;
            throw new InvalidOperationException($"Unsupported OriginModel: {OriginModel.GetType().Name}");
        }
    }

    public override LocString Description => new("powers", "FGO_POWER_NON_STACKABLE_GUTS_POWER.description");

    protected override string SmartDescriptionLocKey => "FGO_POWER_NON_STACKABLE_GUTS_POWER.smartDescription";

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (OriginModel is CardModel card)
                return [HoverTipFactory.FromCard(card)];
            return [];
        }
    }

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            // 右下角: 每次回血量
            ExtraIconAmountLabelSpec.RichText(
                ExtraIconAmountLabelCorner.BottomRight,
                $"[green]{Amount.ToString()}[/green]"),
            // 右上角: 剩余次数
             ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.TopRight,
                $"×{Times}")
        ];
    }

    /// <summary>
    ///     基类 GutsPower 在 BeforeDeath 中治疗并移除自身，但 BeforeDeath 无法阻止死亡。
    ///     这里改为通过 ShouldDie + AfterPreventingDeath 实现真正的复活。
    /// </summary>
    public override Task BeforeDeath(Creature creature)
    {
        return Task.CompletedTask;
    }

    public override bool ShouldDie(Creature creature)
    {
        // 仅当自身所属生物即将死亡、且仍有剩余次数时阻止死亡
        return creature != Owner || Times <= 0;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || Times <= 0)
            return;

        Flash();
        await CreatureCmd.Heal(creature, Amount);
        Times--;
        if (Times <= 0)
            await PowerCmd.Remove(this);
    }
}