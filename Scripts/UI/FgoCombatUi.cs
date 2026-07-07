using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

/// <summary>
///     FGO NP 计量条与按钮 UI。
///     通过 RitsuLib NodeAttachment 系统自动挂载到 NCombatUi，每个战斗 UI 实例一份。
///     NCombatUi ready 时由框架创建并附加为子节点，无需手动调用 AttachTo。
/// </summary>
[RegisterNodeAttachment(
    typeof(NCombatUi),
    "fgo_np_ui",
    NodeName = "FgoNpUi",
    Order = 10,
    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName,
    SetupTiming = NodeAttachmentSetupTiming.AfterAdd)]
public sealed partial class FgoNpUi : Control, INodeAttachmentSetup
{
    private const string NpButtonPath = "res://Fgo/images/charui/np_button.png";
    private const string HpBarBgPath = "res://Fgo/images/ui/health_bar_bg.png";
    private const string HpBarFillPath = "res://Fgo/images/ui/health_bar.png";
    private static readonly Color NpGaugeFillActive = new("00e676");
    private static readonly Color NpGaugeFillDefault = new("2979ff");
    private static readonly Color NpReadyGlow = new("00e676");
    private static readonly Color TextWhite = new("ffffff");

    private NinePatchRect _barBg = null!;
    private NinePatchRect _barFill = null!;
    private bool _built;
    private TextureButton _button = null!;

    private Texture2D? _hpBarBg;
    private Texture2D? _hpBarFill;
    private Label _valueLabel = null!;

    /// <summary>
    ///     节点附加后由框架调用（SetupTiming = AfterAdd 时已加入树）。
    /// </summary>
    public void Setup(Node parent, Node node)
    {
        BuildInternal();
    }

    private void BuildInternal()
    {
        if (_built) return;
        _built = true;

        LoadHealthBarTextures();

        // 根容器铺满父 NCombatUi，但不接收鼠标事件
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false; // 默认隐藏，由 _Process 根据战斗状态切换

        // 背景条 —— 鼠标穿透，否则会挡住按钮
        _barBg = new NinePatchRect
        {
            Name = "NpBarBg",
            Texture = _hpBarBg,
            Modulate = new Color("2a606bc0"),
            PatchMarginLeft = 9,
            PatchMarginRight = 9
        };
        _barBg.AnchorLeft = 0;
        _barBg.AnchorRight = 1;
        _barBg.AnchorTop = 0;
        _barBg.AnchorBottom = 0;
        _barBg.OffsetTop = -26;
        _barBg.OffsetBottom = -6;
        _barBg.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_barBg);

        // 填充条 —— 用 TopLeft + OffsetRight 控制宽度
        _barFill = new NinePatchRect
        {
            Name = "NpBarFill",
            Texture = _hpBarFill,
            PatchMarginLeft = 8,
            PatchMarginRight = 8,
            Modulate = NpGaugeFillDefault
        };
        _barFill.SetAnchorsAndOffsetsPreset(LayoutPreset.TopLeft);
        _barFill.OffsetRight = 0; // 初始宽度 0，由 Refresh 控制
        _barFill.OffsetBottom = 0;
        _barBg.AddChild(_barFill);

        // 数值标签
        _valueLabel = new Label
        {
            Name = "NpValueLabel",
            Text = "0%"
        };
        _valueLabel.AddThemeFontSizeOverride("font_size", 14);
        _valueLabel.AddThemeColorOverride("font_color", TextWhite);
        _valueLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _valueLabel.VerticalAlignment = VerticalAlignment.Center;
        _valueLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _valueLabel.MouseFilter = MouseFilterEnum.Ignore;
        _barBg.AddChild(_valueLabel);

        // NP 按钮 —— 左移 24 像素，强制接收鼠标事件
        _button = new TextureButton
        {
            Name = "NpButton"
        };
        _button.AnchorLeft = 0f;
        _button.AnchorRight = 0f;
        _button.AnchorTop = 0f;
        _button.AnchorBottom = 0f;
        _button.OffsetLeft = -24;
        _button.OffsetRight = 24; // 宽度 = 48
        _button.OffsetTop = -50;
        _button.OffsetBottom = -2;
        _button.IgnoreTextureSize = false;
        _button.StretchMode = TextureButton.StretchModeEnum.Scale;
        _button.MouseFilter = MouseFilterEnum.Stop;

        if (ResourceLoader.Exists(NpButtonPath))
        {
            var tex = GD.Load<Texture2D>(NpButtonPath);
            _button.TextureNormal = tex;
            _button.TextureHover = tex;
            _button.TexturePressed = tex;
            _button.TextureFocused = tex;
            _button.TextureDisabled = tex;
        }

        _button.MouseEntered += () =>
        {
            NHoverTipSet.Remove(_button);
            var tip = new HoverTip(
                new LocString("static_hover_tips", "FGO-NP.title"),
                new LocString("static_hover_tips", "FGO-NP.description")
            );
            NHoverTipSet.CreateAndShow(_button, tip, HoverTipAlignment.Right);
        };
        _button.MouseExited += () => NHoverTipSet.Remove(_button);
        _button.TreeExiting += () => NHoverTipSet.Remove(_button);
        _button.Pressed += OnNpButtonPressed;
        _button.Visible = false;
        AddChild(_button);
    }

    private void LoadHealthBarTextures()
    {
        if (_hpBarBg == null && ResourceLoader.Exists(HpBarBgPath))
            _hpBarBg = GD.Load<Texture2D>(HpBarBgPath);

        if (_hpBarFill == null && ResourceLoader.Exists(HpBarFillPath))
            _hpBarFill = GD.Load<Texture2D>(HpBarFillPath);
    }

    /// <summary>
    ///     根据当前玩家资源刷新 NP 条显示。
    /// </summary>
    public void Refresh(FgoPlayerResources resources)
    {
        if (!_built) return;

        var maxWidth = _barBg.Size.X;
        var ratio = Mathf.Clamp(resources.Np / 100f, 0f, 1f);
        var targetWidth = maxWidth * ratio;
        _barFill.OffsetRight = -(maxWidth - targetWidth);
        _barFill.Modulate = resources.CanUseNp ? NpGaugeFillActive : NpGaugeFillDefault;

        _valueLabel.Text = $"{resources.Np}%";
        _valueLabel.AddThemeColorOverride("font_color", resources.CanUseNp ? NpReadyGlow : TextWhite);

        _button.Visible = resources.CanUseNp;
        _button.Disabled = !resources.CanUseNp;
        _button.Modulate = resources.CanUseNp ? Colors.White : new Color(1f, 1f, 1f, 0.45f);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;
        try
        {
            var inCombat = !CombatManager.Instance.IsPaused &&
                           (CombatManager.Instance.IsInProgress || CombatManager.Instance.IsStarting);
            Visible = inCombat;
            if (!inCombat) return;

            var resources = ModelDb.Singleton<FgoPlayerResources>();
            Refresh(resources);
        }
        catch (Exception e)
        {
            Entry.Logger.Info($"FgoNpUi._Process skipped: {e.Message}");
            SetProcess(false);
        }
    }

    private static async void OnNpButtonPressed()
    {
        try
        {
            var resources = ModelDb.Singleton<FgoPlayerResources>();
            if (!resources.CanUseNp) return;

            resources.SetNpButtonPressed();

            var state = CombatManager.Instance.DebugOnlyGetState();
            var player = LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault(c => c.IsActiveForHooks);
            if (player == null) return;

            var localId = LocalContext.NetId ?? 0;
            var context = new HookPlayerChoiceContext(player, localId, GameActionType.Combat);

            await context.AssignTaskAndWaitForPauseOrCompletion(
                FgoNoblePhantasmCmd.TryChooseNoblePhantasm(context, player));
        }
        catch (Exception e)
        {
            Entry.Logger.Error($"NP button error: {e}");
        }
    }
}

/// <summary>
///     FGO 全局战斗 HUD：暴击星显示与指令咒按钮。
///     通过 RitsuLib NodeAttachment 系统自动挂载到 NCombatUi。
/// </summary>
[RegisterNodeAttachment(
    typeof(NCombatUi),
    "fgo_global_hud",
    NodeName = "FgoGlobalHud",
    Order = 20,
    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName,
    SetupTiming = NodeAttachmentSetupTiming.AfterAdd)]
public sealed partial class FgoGlobalHud : Control, INodeAttachmentSetup
{
    private static readonly Color Gold = new("f3ca49");
    private static readonly Color CritRed = new("ff4444");
    private static readonly Color TextWhite = new("ffffff");
    private bool _built;
    private TextureButton _commandSpellButton = null!;
    private Label _critLabel = null!;

    private Label _starLabel = null!;

    /// <summary>
    ///     节点附加后由框架调用。
    /// </summary>
    public void Setup(Node parent, Node node)
    {
        BuildInternal();
    }

    private void BuildInternal()
    {
        if (_built) return;
        _built = true;

        SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
        Position = new Vector2(-128, 200);
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;

        var vbox = new VBoxContainer
        {
            Name = "GlobalVBox"
        };
        vbox.AddThemeConstantOverride("separation", 10);
        AddChild(vbox);

        BuildCommandSpellButton(vbox);
        _commandSpellButton = vbox.GetNode<TextureButton>("CommandSpellButton");

        BuildStarDisplay(vbox);
        var starContainer = vbox.GetNode<HBoxContainer>("StarContainer");
        _starLabel = starContainer.GetNode<Label>("StarLabel");
        _critLabel = starContainer.GetNode<Label>("CritLabel");
    }

    private void BuildStarDisplay(VBoxContainer parent)
    {
        var container = new HBoxContainer
        {
            Name = "StarContainer"
        };
        container.AddThemeConstantOverride("separation", 4);

        container.MouseEntered += () =>
        {
            NHoverTipSet.Remove(container);
            var tip = new HoverTip(
                new LocString("static_hover_tips", "FGO-STAR.title"),
                new LocString("static_hover_tips", "FGO-STAR.description")
            );
            NHoverTipSet.CreateAndShow(container, tip, HoverTipAlignment.Right);
        };
        container.MouseExited += () => NHoverTipSet.Remove(container);
        container.TreeExiting += () => NHoverTipSet.Remove(container);

        parent.AddChild(container);

        var starIcon = new Label
        {
            Name = "StarIcon",
            Text = "✨"
        };
        starIcon.AddThemeFontSizeOverride("font_size", 64);
        starIcon.AddThemeColorOverride("font_color", Gold);
        container.AddChild(starIcon);

        var starLabel = new Label
        {
            Name = "StarLabel",
            Text = "0"
        };
        starLabel.AddThemeFontSizeOverride("font_size", 64);
        starLabel.AddThemeColorOverride("font_color", TextWhite);
        container.AddChild(starLabel);

        var critLabel = new Label
        {
            Name = "CritLabel",
            Text = "CRIT!"
        };
        critLabel.AddThemeFontSizeOverride("font_size", 14);
        critLabel.AddThemeColorOverride("font_color", CritRed);
        critLabel.Visible = false;
        container.AddChild(critLabel);
    }

    private void BuildCommandSpellButton(VBoxContainer parent)
    {
        var button = new TextureButton
        {
            Name = "CommandSpellButton",
            CustomMinimumSize = new Vector2(128f, 128f),
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered
        };

        var path = GetCommandSpellImagePath(3);
        if (ResourceLoader.Exists(path))
            button.TextureNormal = GD.Load<Texture2D>(path);

        var bgStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.8f, 0.6f, 0.0f, 0.9f)
        };
        bgStyle.SetCornerRadiusAll(8);
        bgStyle.BorderWidthLeft = 2;
        bgStyle.BorderWidthRight = 2;
        bgStyle.BorderWidthTop = 2;
        bgStyle.BorderWidthBottom = 2;
        bgStyle.BorderColor = new Color(1f, 0.84f, 0f);
        button.AddThemeStyleboxOverride("normal", bgStyle);

        var hoverStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.9f, 0.7f, 0.1f, 0.9f)
        };
        hoverStyle.SetCornerRadiusAll(8);
        hoverStyle.BorderWidthLeft = 2;
        hoverStyle.BorderWidthRight = 2;
        hoverStyle.BorderWidthTop = 2;
        hoverStyle.BorderWidthBottom = 2;
        hoverStyle.BorderColor = new Color(1f, 0.84f, 0f);
        button.AddThemeStyleboxOverride("hover", hoverStyle);

        button.MouseEntered += () =>
        {
            NHoverTipSet.Remove(button);
            var tip = new HoverTip(
                new LocString("static_hover_tips", "FGO-COMMAND_SPELL.title"),
                new LocString("static_hover_tips", "FGO-COMMAND_SPELL.description")
            );
            NHoverTipSet.CreateAndShow(button, tip, HoverTipAlignment.Right);
        };
        button.MouseExited += () => NHoverTipSet.Remove(button);
        button.TreeExiting += () => NHoverTipSet.Remove(button);

        button.Pressed += OnCommandSpellButtonPressed;
        parent.AddChild(button);
    }

    /// <summary>
    ///     根据当前玩家资源刷新 HUD 显示。
    /// </summary>
    public void Refresh(FgoPlayerResources resources)
    {
        if (!_built) return;

        _starLabel.Text = resources.Stars.ToString();
        _starLabel.AddThemeColorOverride("font_color", resources.CanCrit ? Gold : TextWhite);

        _critLabel.Visible = resources.CanCrit;

        var path = GetCommandSpellImagePath(resources.CommandSpell);
        var tex = ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
        if (tex != null) _commandSpellButton.TextureNormal = tex;
        _commandSpellButton.Modulate = resources.CommandSpell > 0
            ? new Color(1f, 1f, 1f)
            : new Color(1f, 1f, 1f, 0.4f);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;
        try
        {
            var inCombat = !CombatManager.Instance.IsPaused &&
                           (CombatManager.Instance.IsInProgress || CombatManager.Instance.IsStarting);
            Visible = inCombat;
            if (!inCombat) return;

            var resources = ModelDb.Singleton<FgoPlayerResources>();
            Refresh(resources);
        }
        catch (Exception e)
        {
            Entry.Logger.Info($"FgoGlobalHud._Process skipped: {e.Message}");
            SetProcess(false);
        }
    }

    private static string GetCommandSpellImagePath(int count)
    {
        return $"res://Fgo/images/ui/CommandSpell/CommandSpell{Math.Clamp(count, 0, 3)}.png";
    }

    private static async void OnCommandSpellButtonPressed()
    {
        try
        {
            var resources = ModelDb.Singleton<FgoPlayerResources>();
            if (!resources.CanUseCommandSpell) return;

            var state = CombatManager.Instance.DebugOnlyGetState();
            var player = LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault(c => c.IsActiveForHooks);
            if (player == null) return;

            var localId = LocalContext.NetId ?? 0;
            var context = new HookPlayerChoiceContext(player, localId, GameActionType.Combat);

            await context.AssignTaskAndWaitForPauseOrCompletion(
                FgoCommandSpellCmd.TryUseCommandSpell(context, player));
        }
        catch (Exception e)
        {
            Entry.Logger.Error($"CommandSpell button error: {e}");
        }
    }
}