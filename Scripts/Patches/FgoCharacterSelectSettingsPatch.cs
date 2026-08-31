using System;
using System.Runtime.CompilerServices;
using Fgo.Scripts.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Settings;

namespace Fgo.Scripts.Patches;

/// <summary>
///     选人页设置面板：当玩家在 <see cref="NCharacterSelectScreen" /> 选中 FGO 角色时，于屏幕右侧显示
///     一个修改 <see cref="FgoReflectedSettings" /> 的面板——一个整数滑块（每费用 NP 获取）+ 两个开关
///     （宝具 0 费、圣诞彩蛋）。原为 Saya 皮肤选择器移植，这里只保留「修改设置的滑块与按钮」，去掉皮肤切换。
///     通过 RitsuLib 的 IPatchMethod 模式注册（见 Entry.Init），不直接用 Harmony.PatchAll。
/// </summary>

public sealed class FgoCharacterSelectSettingsReadyPatch : IPatchMethod
{
    public static string PatchId => "fgo.character_select.settings_ready";
    public static string Description => "Create the FGO settings panel on the character select screen";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCharacterSelectScreen>(nameof(NCharacterSelectScreen._Ready))];

    [HarmonyPostfix]
    public static void Postfix(NCharacterSelectScreen __instance)
        => FgoSettingsPanelRegistry.Get(__instance).Refresh(false);
}

public sealed class FgoCharacterSelectSettingsSelectPatch : IPatchMethod
{
    public static string PatchId => "fgo.character_select.settings_select";
    public static string Description => "Show the FGO settings panel when an FGO character is selected";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCharacterSelectScreen>(nameof(NCharacterSelectScreen.SelectCharacter))];

    [HarmonyPostfix]
    public static void Postfix(
        NCharacterSelectScreen __instance,
        NCharacterSelectButton charSelectButton,
        CharacterModel characterModel)
    {
        bool show = characterModel is FgoCharacter
            && !charSelectButton.IsRandom
            && !charSelectButton.IsLocked;
        FgoSettingsPanelRegistry.Get(__instance).Refresh(show);
    }
}

public sealed class FgoCharacterSelectSettingsClosedPatch : IPatchMethod
{
    public static string PatchId => "fgo.character_select.settings_closed";
    public static string Description => "Hide the FGO settings panel when the character select closes";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCharacterSelectScreen>(nameof(NCharacterSelectScreen.OnSubmenuClosed))];

    [HarmonyPostfix]
    public static void Postfix(NCharacterSelectScreen __instance)
        => FgoSettingsPanelRegistry.Get(__instance).Refresh(false);
}

internal static class FgoSettingsPanelRegistry
{
    private static readonly ConditionalWeakTable<NCharacterSelectScreen, FgoSettingsPanel> Panels = new();

    public static FgoSettingsPanel Get(NCharacterSelectScreen screen) =>
        Panels.GetValue(screen, static s => new FgoSettingsPanel(s));
}

internal sealed class FgoSettingsPanel
{
    private const float PanelWidth = 360f;
    private const float PanelHeight = 250f;
    private const float EdgePadding = 24f;
    private const float VerticalCenterRatio = 0.5f;

    private static readonly Color Gold = new("c47e09");
    private static readonly Color PanelBg = new(0.06f, 0.04f, 0.02f, 0.92f);

    private readonly PanelContainer _root;
    private readonly HSlider _npSlider;
    private readonly Label _npValueLabel;
    private readonly CheckButton _noCostToggle;
    private readonly CheckButton _padoruToggle;

    public FgoSettingsPanel(NCharacterSelectScreen screen)
    {
        _root = CreateRoot();
        _npSlider = CreateSlider();
        _npValueLabel = CreateLabel(string.Empty, 40);
        _noCostToggle = new CheckButton();
        _padoruToggle = new CheckButton();

        BuildLayout();
        screen.AddChild(_root);
    }

    public void Refresh(bool show)
    {
        _root.Visible = show;
        if (!show)
            return;

        FgoReflectedSettings.ReflectBoundValues();

        if (FgoReflectedSettings.TryGetIntBinding(FgoReflectedSettings.BaseNpPerCostEntryId, out var npBinding))
        {
            int value = npBinding.Read();
            _npSlider.SetValueNoSignal(value);
            _npValueLabel.Text = value.ToString();
        }

        if (FgoReflectedSettings.TryGetToggleBinding(
                FgoReflectedSettings.EnableNoCostNoblePhantasmEntryId, out var noCostBinding))
            _noCostToggle.SetPressedNoSignal(noCostBinding.Read());

        if (FgoReflectedSettings.TryGetToggleBinding(FgoReflectedSettings.EnablePadoruEntryId, out var padoruBinding))
            _padoruToggle.SetPressedNoSignal(padoruBinding.Read());
    }

    private void OnNpValueChanged(double value)
    {
        var clamped = (int)Math.Round(value);
        if (FgoReflectedSettings.TryGetIntBinding(FgoReflectedSettings.BaseNpPerCostEntryId, out var binding))
        {
            binding.Write(clamped);
            _npValueLabel.Text = clamped.ToString();
        }
    }

    private static void OnNoCostToggled(bool value)
    {
        if (FgoReflectedSettings.TryGetToggleBinding(
                FgoReflectedSettings.EnableNoCostNoblePhantasmEntryId, out var binding))
            binding.Write(value);
    }

    private static void OnPadoruToggled(bool value)
    {
        if (FgoReflectedSettings.TryGetToggleBinding(FgoReflectedSettings.EnablePadoruEntryId, out var binding))
            binding.Write(value);
    }

    private void BuildLayout()
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        _root.AddChild(margin);

        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 14);
        margin.AddChild(column);

        var title = CreateLabel(GetLoc("FGO_SETTINGS_UI_GENERAL.title"), 0);
        title.AddThemeColorOverride("font_color", Gold);
        title.AddThemeFontSizeOverride("font_size", 20);
        column.AddChild(title);

        column.AddChild(BuildSliderGroup());
        column.AddChild(BuildToggleRow(
            GetLoc("FGO_SETTINGS_UI_ENABLE_NO_COST_NOBLE_PHANTASM.title"), _noCostToggle, OnNoCostToggled));
        column.AddChild(BuildToggleRow(GetLoc("FGO_SETTINGS_UI_ENABLE_PADORU.title"), _padoruToggle, OnPadoruToggled));
    }

    private Control BuildSliderGroup()
    {
        var group = new VBoxContainer();
        group.AddThemeConstantOverride("separation", 4);

        var header = new HBoxContainer();
        var label = CreateLabel(GetLoc("FGO_SETTINGS_UI_BASE_NP_PER_COST.title"), 0);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _npValueLabel.HorizontalAlignment = HorizontalAlignment.Right;

        header.AddChild(label);
        header.AddChild(_npValueLabel);
        group.AddChild(header);
        group.AddChild(_npSlider);
        return group;
    }

    private static Control BuildToggleRow(string text, CheckButton toggle, Action<bool> onToggled)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);

        var label = CreateLabel(text, 0);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        toggle.Toggled += value => onToggled(value);

        row.AddChild(label);
        row.AddChild(toggle);
        return row;
    }

    private HSlider CreateSlider()
    {
        var slider = new HSlider
        {
            MinValue = 0,
            MaxValue = 10,
            Step = 1,
            Value = 5,
            CustomMinimumSize = new Vector2(1f, 20f),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        slider.ValueChanged += OnNpValueChanged;
        return slider;
    }

    private static Label CreateLabel(string text, float width)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.92f));
        if (width > 0f)
            label.CustomMinimumSize = new Vector2(width, 24f);
        return label;
    }

    private static PanelContainer CreateRoot()
    {
        var root = new PanelContainer
        {
            Name = "FgoCharacterSelectSettings",
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        root.AnchorLeft = 1f;
        root.AnchorTop = VerticalCenterRatio;
        root.AnchorRight = 1f;
        root.AnchorBottom = VerticalCenterRatio;
        root.GrowHorizontal = Control.GrowDirection.Begin;
        root.GrowVertical = Control.GrowDirection.Begin;
        root.OffsetLeft = -PanelWidth - EdgePadding;
        root.OffsetTop = -PanelHeight * 0.5f;
        root.OffsetRight = -EdgePadding;
        root.OffsetBottom = PanelHeight * 0.5f;

        var style = new StyleBoxFlat
        {
            BgColor = PanelBg,
            BorderColor = Gold
        };
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(10);
        root.AddThemeStyleboxOverride("panel", style);
        return root;
    }

    private static string GetLoc(string key) =>
        new LocString("settings_ui", key).GetFormattedText();
}