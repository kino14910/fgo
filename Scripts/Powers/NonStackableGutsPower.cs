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

public abstract class NonStackableGutsPower : GutsPower, IPowerExtraIconAmountLabelSpecsProvider
{
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/GutsPower.png",
        "res://Fgo/images/powers/big/GutsPower.png"
    );

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

    public override bool ShouldDie(Creature creature) =>
        creature != Owner || Times <= 0;

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