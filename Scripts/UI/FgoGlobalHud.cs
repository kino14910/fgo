using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
// ReSharper disable once RedundantUsingDirective
using Fgo.Scripts;

namespace Fgo.Scripts.UI;

public sealed partial class FgoGlobalHud : Control
{
    private static readonly HoverTip CommandSpellHoverTip = new(
        new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_COMMAND_SPELL.title"),
        new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_COMMAND_SPELL.description"));

    private static readonly HoverTip StarHoverTip = new(
        new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_STAR.title"),
        new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_STAR.description"));

    private static bool _lastVisible;
    private TextureButton _commandSpellButton = null!;
    private HBoxContainer _starBox = null!;
    private Label _starLabel = null!;

    public static void Initialize()
    {
        ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NCombatUi, FgoGlobalHud>(
                "global_hud",
                static _ => new FgoGlobalHud(),
                static (_, hud) => hud.Bind(),
                new NodeAttachmentOptions
                {
                    Name = "FgoGlobalHud",
                    DuplicatePolicy =
                        NodeAttachmentDuplicatePolicy.ReuseExistingByName
                });
    }

    public void Bind()
    {
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var vbox = new VBoxContainer();
        vbox.Name = "Root";
        vbox.SetAnchorsPreset(LayoutPreset.TopRight);
        vbox.OffsetLeft = -220;
        vbox.OffsetRight = 0;
        vbox.OffsetTop = 160;
        vbox.OffsetBottom = 560;
        vbox.Alignment = BoxContainer.AlignmentMode.Begin;
        vbox.AddThemeConstantOverride("separation", 24);

        AddChild(vbox);

        //---------------------------------------
        // Command Spell
        //---------------------------------------

        _commandSpellButton = new TextureButton();

        _commandSpellButton.CustomMinimumSize =
            new Vector2(128, 128);

        _commandSpellButton.StretchMode =
            TextureButton.StretchModeEnum.KeepAspectCentered;

        _commandSpellButton.Pressed += OnCommandSpellButtonPressed;
        _commandSpellButton.MouseEntered += OnCommandSpellMouseEntered;
        _commandSpellButton.MouseExited += OnCommandSpellMouseExited;

        vbox.AddChild(_commandSpellButton);

        //---------------------------------------
        // Star
        //---------------------------------------

        var hbox = new HBoxContainer();

        hbox.AddThemeConstantOverride("separation", 8);
        // 整个 star 行作为 hover tip 的载体
        hbox.MouseFilter = MouseFilterEnum.Stop;
        hbox.MouseEntered += OnStarMouseEntered;
        hbox.MouseExited += OnStarMouseExited;
        _starBox = hbox;

        vbox.AddChild(hbox);

        var starIcon = new Label();

        starIcon.Text = "✨";
        // 让父 HBoxContainer 接收鼠标事件
        starIcon.MouseFilter = MouseFilterEnum.Pass;

        starIcon.AddThemeFontSizeOverride(
            "font_size",
            48);

        hbox.AddChild(starIcon);

        _starLabel = new Label();

        _starLabel.Text = "0";
        // 让父 HBoxContainer 接收鼠标事件
        _starLabel.MouseFilter = MouseFilterEnum.Pass;

        _starLabel.AddThemeFontSizeOverride(
            "font_size",
            42);

        hbox.AddChild(_starLabel);

        Visible = false;
    }

    public override void _Process(double delta)
    {
        FgoCombatUi.Update();
    }

    public static void SetHudVisible(bool visible)
    {
        if (visible == _lastVisible) return;
        var huds = FindAll().ToList();
        _lastVisible = visible;
        foreach (var hud in huds) hud.Visible = visible;
    }

    public static void Update()
    {
        var resources =
            ModelDb.Singleton<FgoPlayerResources>();

        var huds = FindAll().ToList();
        foreach (var hud in huds) hud.Refresh(resources);
    }

    private void Refresh(FgoPlayerResources resources)
    {
        _starLabel.Text = resources.Stars.ToString();
        _commandSpellButton.TextureNormal = GD.Load<Texture2D>(
            $"res://Fgo/images/ui/CommandSpell/CommandSpell{Math.Clamp(resources.CommandSpell, 0, 3)}.png");

        var canUse = resources.CanUseCommandSpell;
        _commandSpellButton.Modulate = canUse ? Colors.White : new Color(1, 1, 1, 0.35f);
        _commandSpellButton.Disabled = !canUse;
    }

    public override void _ExitTree()
    {
        _commandSpellButton.Pressed -= OnCommandSpellButtonPressed;
        _commandSpellButton.MouseEntered -= OnCommandSpellMouseEntered;
        _commandSpellButton.MouseExited -= OnCommandSpellMouseExited;
        NHoverTipSet.Remove(_commandSpellButton);

        _starBox.MouseEntered -= OnStarMouseEntered;
        _starBox.MouseExited -= OnStarMouseExited;
        NHoverTipSet.Remove(_starBox);
    }

    private static async void OnCommandSpellButtonPressed()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault();
        if (player == null) return;

        var choiceContext = new BlockingPlayerChoiceContext();
        await FgoCommandSpellCmd.TryUseCommandSpell(choiceContext, player);
    }

    private void OnCommandSpellMouseEntered()
    {
        NHoverTipSet.CreateAndShow(_commandSpellButton, CommandSpellHoverTip, HoverTipAlignment.Left);
    }

    private void OnCommandSpellMouseExited()
    {
        NHoverTipSet.Remove(_commandSpellButton);
    }

    private void OnStarMouseEntered()
    {
        NHoverTipSet.CreateAndShow(_starBox, StarHoverTip, HoverTipAlignment.Left);
    }

    private void OnStarMouseExited()
    {
        NHoverTipSet.Remove(_starBox);
    }

    private static IEnumerable<FgoGlobalHud> FindAll()
    {
        var tree = (SceneTree)Engine.GetMainLoop();

        foreach (var hud in Find(tree.Root))
            yield return hud;
    }

    private static IEnumerable<FgoGlobalHud> Find(Node node)
    {
        if (node is FgoGlobalHud hud)
            yield return hud;

        foreach (var child in node.GetChildren())
        foreach (var h in Find(child))
            yield return h;
    }
}