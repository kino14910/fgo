using System.Runtime.CompilerServices;
using Fgo.Scripts.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2RitsuLib.Patching.Models;

namespace Fgo.Scripts.Patches;

/// <summary>
///     选人页设置面板：当玩家在 <see cref="NCharacterSelectScreen" /> 选中 FGO 角色时，于屏幕右侧显示
///     一个修改 <see cref="FgoReflectedSettings" /> 的面板——一个整数滑块（每费用 NP 获取）+ 两个开关
///     （宝具 0 费、圣诞彩蛋）。仅保留修改设置的滑块与开关。
///     通过 RitsuLib 的 IPatchMethod 模式注册（见 Entry.Init），不直接用 Harmony.PatchAll。
/// </summary>
public sealed class FgoCharacterSelectSettingsReadyPatch : IPatchMethod
{
    public static string PatchId => "fgo.character_select.settings_ready";
    public static string Description => "Create the FGO settings panel on the character select screen";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [PatchTarget.Method<NCharacterSelectScreen>(nameof(NCharacterSelectScreen._Ready))];
    }

    [HarmonyPostfix]
    public static void Postfix(NCharacterSelectScreen __instance)
    {
        FgoSettingsPanelRegistry.Get(__instance).Refresh(false);
    }
}

public sealed class FgoCharacterSelectSettingsSelectPatch : IPatchMethod
{
    public static string PatchId => "fgo.character_select.settings_select";
    public static string Description => "Show the FGO settings panel when an FGO character is selected";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [PatchTarget.Method<NCharacterSelectScreen>(nameof(NCharacterSelectScreen.SelectCharacter))];
    }

    [HarmonyPostfix]
    public static void Postfix(
        NCharacterSelectScreen __instance,
        NCharacterSelectButton charSelectButton,
        CharacterModel characterModel)
    {
        var show = characterModel is FgoCharacter
                   && charSelectButton is { IsRandom: false, IsLocked: false };
        if (show)
        {
            FgoReflectedSettings.ReflectBoundValues();
            FgoSkinApplier.ApplySkinToSelectPreview(__instance, (int)FgoReflectedSettings.CharacterSkin);
        }

        FgoSettingsPanelRegistry.Get(__instance).Refresh(show);
    }
}

public sealed class FgoCharacterSelectSettingsClosedPatch : IPatchMethod
{
    public static string PatchId => "fgo.character_select.settings_closed";
    public static string Description => "Hide the FGO settings panel when the character select closes";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [PatchTarget.Method<NCharacterSelectScreen>(nameof(NCharacterSelectScreen.OnSubmenuClosed))];
    }

    [HarmonyPostfix]
    public static void Postfix(NCharacterSelectScreen __instance)
    {
        FgoSettingsPanelRegistry.Get(__instance).Refresh(false);
    }
}

internal static class FgoSettingsPanelRegistry
{
    private static readonly ConditionalWeakTable<NCharacterSelectScreen, FgoSettingsPanel> Panels = new();

    public static FgoSettingsPanel Get(NCharacterSelectScreen screen)
    {
        return Panels.GetValue(screen, static s => new FgoSettingsPanel(s));
    }
}

internal sealed class FgoSettingsPanel
{
    private const float PanelWidth = 360f;
    private const float PanelHeight = 500f;
    private const float EdgePadding = 24f;
    private const float VerticalCenterRatio = 0.5f;
    private const float SkinListMaxHeight = 240f;
    private const int SkinLayerIndex = 128;

    private static readonly Color Gold = new("c47e09");
    private static readonly Color PanelBg = new(0.06f, 0.04f, 0.02f, 0.92f);
    private static readonly Color SkinBorderColor = new(0.8f, 0.8f, 0.8f);
    private readonly CheckButton _noCostToggle;
    private readonly HSlider _npSlider;
    private readonly Label _npValueLabel;
    private readonly CheckButton _padoruToggle;
    private readonly PanelContainer _root;

    private readonly NCharacterSelectScreen _screen;
    private readonly Button _skinHeader;
    private readonly CanvasLayer _skinLayer;
    private readonly Control _skinLayerBlocker;
    private readonly Control _skinLayerHost;
    private readonly TextureRect _skinPreview;
    private readonly ScrollContainer _skinScroll;
    private int _committedSkin;

    public FgoSettingsPanel(NCharacterSelectScreen screen)
    {
        _screen = screen;
        _root = CreateRoot();
        _npSlider = CreateSlider();
        _npValueLabel = CreateLabel(string.Empty, 40);
        _noCostToggle = new CheckButton();
        _padoruToggle = new CheckButton();
        _skinHeader = CreateSkinHeader();
        _skinScroll = CreateSkinList();
        _skinPreview = CreateSkinPreview();

        // 展开列表挂在独立的高层 CanvasLayer 上：浮在全局最顶层，且不参与面板容器的布局计算。
        _skinLayer = new CanvasLayer { Layer = SkinLayerIndex, Visible = false };
        _skinLayerBlocker = new Control { MouseFilter = Control.MouseFilterEnum.Stop };
        _skinLayerBlocker.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _skinLayerBlocker.GuiInput += OnSkinLayerClickedOutside;
        _skinLayerHost = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        _skinScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _skinLayerHost.AddChild(_skinScroll);
        _skinLayer.AddChild(_skinLayerBlocker);
        _skinLayer.AddChild(_skinLayerHost);

        BuildLayout();
        screen.AddChild(_root);
        screen.AddChild(_skinLayer);
    }

    public void Refresh(bool show)
    {
        _root.Visible = show;
        if (!show)
        {
            CloseSkinList();
            return;
        }

        FgoReflectedSettings.ReflectBoundValues();

        if (FgoReflectedSettings.TryGetIntBinding(FgoReflectedSettings.BaseNpPerCostEntryId, out var npBinding))
        {
            var value = npBinding.Read();
            _npSlider.SetValueNoSignal(value);
            _npValueLabel.Text = value.ToString();
        }

        if (FgoReflectedSettings.TryGetToggleBinding(
                FgoReflectedSettings.EnableNoCostNoblePhantasmEntryId, out var noCostBinding))
            _noCostToggle.SetPressedNoSignal(noCostBinding.Read());

        if (FgoReflectedSettings.TryGetToggleBinding(FgoReflectedSettings.EnablePadoruEntryId, out var padoruBinding))
            _padoruToggle.SetPressedNoSignal(padoruBinding.Read());

        if (FgoReflectedSettings.TryGetSkinBinding(out var skinBinding))
        {
            _committedSkin = (int)skinBinding.Read();
            _skinHeader.Text = $"{_committedSkin} - {FgoSkinApplier.SkinNames[_committedSkin]}";
            _skinPreview.Texture = FgoSkinApplier.LoadSkinTexture(_committedSkin);
            FgoSkinApplier.ApplySkinToSelectPreview(_screen, _committedSkin);
        }

        CloseSkinList();
    }

    private void OnSkinPicked(int index)
    {
        if (FgoReflectedSettings.TryGetSkinBinding(out var binding))
        {
            binding.Write((CharacterSkinId)index);
            _committedSkin = index;
            _skinHeader.Text = $"{index} - {FgoSkinApplier.SkinNames[index]}";
        }

        CloseSkinList();
    }

    private void OnSkinHovered(int index)
    {
        _skinPreview.Texture = FgoSkinApplier.LoadSkinTexture(index);
        FgoSkinApplier.ApplySkinToSelectPreview(_screen, index);
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
        column.AddChild(BuildSkinGroup());
    }

    private Control BuildSkinGroup()
    {
        var group = new VBoxContainer();
        group.AddThemeConstantOverride("separation", 6);
        group.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var label = CreateLabel("人物皮肤", 0);
        label.AddThemeColorOverride("font_color", Gold);
        group.AddChild(label);

        _skinPreview.CustomMinimumSize = new Vector2(240f, 240f);
        group.AddChild(_skinPreview);

        group.AddChild(_skinHeader);

        return group;
    }

    private static StyleBoxFlat SkinBorderStyle()
    {
        var sb = new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0f),
            BorderColor = SkinBorderColor
        };
        sb.SetBorderWidthAll(1);
        sb.SetCornerRadiusAll(6);
        sb.SetContentMarginAll(6);
        return sb;
    }

    private Button CreateSkinHeader()
    {
        var header = new Button
        {
            Text = "0 - 迦勒底（默认）",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        header.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.92f));
        var border = SkinBorderStyle();
        header.AddThemeStyleboxOverride("normal", border);
        header.AddThemeStyleboxOverride("hover", border);
        header.AddThemeStyleboxOverride("pressed", border);
        header.AddThemeStyleboxOverride("focus", border);
        header.Pressed += ToggleSkinList;
        return header;
    }

    // 自定义下拉列表：纯 Control（不弹原生 PopupMenu 窗口），避免选人界面在弹窗打开时暂停 BGM。
    private ScrollContainer CreateSkinList()
    {
        var scroll = new ScrollContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        // 边框画在 ScrollContainer 自身的 "panel" 上：可视区外框固定不动，不会随内容滚动。
        var bg = new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.08f, 0.05f, 0.98f),
            BorderColor = SkinBorderColor
        };
        bg.SetBorderWidthAll(1);
        bg.SetCornerRadiusAll(6);
        bg.SetContentMarginAll(4);
        scroll.AddThemeStyleboxOverride("panel", bg);

        var list = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        list.AddThemeConstantOverride("separation", 2);

        for (var i = 0; i < FgoSkinApplier.SkinCount; i++)
        {
            var idx = i;
            var item = new Button
            {
                Text = $"{i} - {FgoSkinApplier.SkinNames[i]}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            item.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.92f));
            item.MouseEntered += () => OnSkinHovered(idx);
            item.Pressed += () => OnSkinPicked(idx);
            list.AddChild(item);
        }

        scroll.AddChild(list);
        return scroll;
    }

    private void ToggleSkinList()
    {
        if (_skinLayer.Visible)
        {
            CloseSkinList();
            return;
        }

        PositionSkinList();
        _skinLayer.Visible = true;
    }

    private void CloseSkinList()
    {
        _skinLayer.Visible = false;
        RestoreSkinPreview();
    }

    private void OnSkinLayerClickedOutside(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true })
            CloseSkinList();
    }

    // 列表浮层不参与布局，所以按表头的屏幕矩形手动定位：默认挂在表头下方，空间不够时向上翻转。
    private void PositionSkinList()
    {
        var header = _skinHeader.GetGlobalRect();
        var viewport = _screen.GetViewport().GetVisibleRect().Size;

        var width = Mathf.Max(header.Size.X, 120f);
        var y = header.Position.Y + header.Size.Y + 4f;
        var height = Mathf.Min(SkinListMaxHeight, viewport.Y - y - 8f);

        if (height < 80f)
        {
            height = Mathf.Min(SkinListMaxHeight, Mathf.Max(header.Position.Y - 8f, 80f));
            y = Mathf.Max(header.Position.Y - height - 4f, 0f);
        }

        _skinLayerHost.Position = new Vector2(header.Position.X, y);
        _skinLayerHost.Size = new Vector2(width, height);
    }

    private void RestoreSkinPreview()
    {
        _skinPreview.Texture = FgoSkinApplier.LoadSkinTexture(_committedSkin);
        FgoSkinApplier.ApplySkinToSelectPreview(_screen, _committedSkin);
    }

    private static TextureRect CreateSkinPreview()
    {
        return new TextureRect
        {
            // IgnoreSize：预览框尺寸固定（不随纹理比例变化），任何比例的皮肤图都在框内等比居中。
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
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
        root.OffsetTop = -PanelHeight * 0.4f;
        root.OffsetRight = -EdgePadding;
        root.OffsetBottom = PanelHeight * 0.35f;

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

    private static string GetLoc(string key)
    {
        return new LocString("settings_ui", key).GetFormattedText();
    }
}