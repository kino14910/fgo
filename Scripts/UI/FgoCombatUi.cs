using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

public static class FgoCombatUi
{
    public static void Initialize()
    {
        FgoGlobalHud.Initialize();
        // FgoCardNpUi.Initialize();
        FgoNpBar.Initialize();
        EnergyCounterPivotFix.Initialize();
    }

    public static void Update()
    {
        if (CombatManager.Instance == null)
            return;

        bool inCombat =
            !CombatManager.Instance.IsPaused &&
            (CombatManager.Instance.IsInProgress ||
             CombatManager.Instance.IsStarting);

        FgoGlobalHud.SetVisible(inCombat);

        if (!inCombat)
            return;

        FgoGlobalHud.Update();
    }
}

/// <summary>
///     修正 RitsuLib 包装器未继承 pivot_offset 的问题：
///     RitsuNEnergyCounterNodeFactory.CreateFullRectControl 创建的 wrapper
///     PivotOffset 默认为 (0,0)，导致旋转中心在左上角。
/// </summary>
sealed partial class EnergyCounterPivotFix : Node
{
    public static void Initialize()
    {
        ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NEnergyCounter, EnergyCounterPivotFix>(
                "energy_pivot_fix",
                static _ => new EnergyCounterPivotFix(),
                new NodeAttachmentOptions
                {
                    Name = "EnergyCounterPivotFix",
                    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName,
                });
    }

    public override void _Ready()
    {
        var rotationLayers = GetParent()?.GetNodeOrNull<Control>("%RotationLayers");
        if (rotationLayers != null)
            rotationLayers.PivotOffset = new Vector2(64, 64);
    }
}