using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

// ReSharper disable once RedundantUsingDirective
using Fgo.Scripts;

namespace Fgo.Scripts.UI;

public sealed partial class FgoGlobalHud : Control
{
    private Label _starLabel = null!;
    private Label _critLabel = null!;
    private TextureButton _commandSpellButton = null!;

    public static void Initialize()
    {
        Entry.Logger.Info("[FgoGlobalHud] Initialize() called - registering NCombatUi attachment");
        ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NCombatUi, FgoGlobalHud>(
                "global_hud",
                static _ =>
                {
                    Entry.Logger.Info("[FgoGlobalHud] factory: creating new FgoGlobalHud instance");
                    return new FgoGlobalHud();
                },
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
        Entry.Logger.Info($"[FgoGlobalHud] Bind() called, parent={GetParent()?.GetType().Name ?? "null"}");
    }

    public override void _Ready()
    {
        Entry.Logger.Info($"[FgoGlobalHud] _Ready() called, parent={GetParent()?.GetType().Name ?? "null"}");

        MouseFilter = MouseFilterEnum.Ignore;

        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Log parent size for debugging
        var parent = GetParent();
        if (parent is Control parentCtrl)
            Entry.Logger.Info($"[FgoGlobalHud] parent size={parentCtrl.Size}, parent visible={parentCtrl.Visible}");
        else
            Entry.Logger.Info($"[FgoGlobalHud] parent is {parent?.GetType().Name ?? "null"} (not a Control)");

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

        Entry.Logger.Info($"[FgoGlobalHud] vbox added, self size={Size}, self visible={Visible}");

        //---------------------------------------
        // Command Spell
        //---------------------------------------

        _commandSpellButton = new TextureButton();

        _commandSpellButton.CustomMinimumSize =
            new Vector2(128, 128);

        _commandSpellButton.StretchMode =
            TextureButton.StretchModeEnum.KeepAspectCentered;

        _commandSpellButton.Pressed += OnCommandSpellButtonPressed;

        vbox.AddChild(_commandSpellButton);

        //---------------------------------------
        // Star
        //---------------------------------------

        var hbox = new HBoxContainer();

        hbox.AddThemeConstantOverride("separation", 8);

        vbox.AddChild(hbox);

        var starIcon = new Label();

        starIcon.Text = "✨";

        starIcon.AddThemeFontSizeOverride(
            "font_size",
            48);

        hbox.AddChild(starIcon);

        _starLabel = new Label();

        _starLabel.Text = "0";

        _starLabel.AddThemeFontSizeOverride(
            "font_size",
            42);

        hbox.AddChild(_starLabel);

        _critLabel = new Label();

        _critLabel.Text = "CRIT";

        _critLabel.Visible = false;

        _critLabel.AddThemeFontSizeOverride(
            "font_size",
            22);

        hbox.AddChild(_critLabel);

        Visible = false;
    }

    public override void _Process(double delta)
    {
        FgoCombatUi.Update();
    }

    private static bool _lastVisible;
    private static int _lastLoggedNp = -1;

    public static void SetVisible(bool visible)
    {
        var huds = FindAll().ToList();
        if (visible != _lastVisible)
        {
            _lastVisible = visible;
            foreach (var hud in huds)
            {
                hud.Visible = visible;
            }
        }
    }

    public static void Update()
    {
        var resources =
            ModelDb.Singleton<FgoPlayerResources>();

        var huds = FindAll().ToList();
        if (huds.Count > 0 && resources != null && resources.Np != _lastLoggedNp)
        {
            Entry.Logger.Info($"[FgoGlobalHud] Update() found {huds.Count} hud(s), np={resources.Np}, stars={resources.Stars}");
            _lastLoggedNp = resources.Np;
        }
        foreach (var hud in huds)
        {
            hud.Refresh(resources!);
        }
    }

    private void Refresh(FgoPlayerResources resources)
    {
        _starLabel.Text = resources.Stars.ToString();
        _critLabel.Visible = resources.CanCrit;
        _commandSpellButton.TextureNormal = GD.Load<Texture2D>(
            $"res://Fgo/images/ui/CommandSpell/CommandSpell{Math.Clamp(resources.CommandSpell, 0, 3)}.png");

        var canUse = resources.CanUseCommandSpell;
        _commandSpellButton.Modulate = canUse ? Colors.White : new Color(1, 1, 1, 0.35f);
        _commandSpellButton.Disabled = !canUse;
    }

    public override void _ExitTree()
    {
        if (_commandSpellButton != null)
            _commandSpellButton.Pressed -= OnCommandSpellButtonPressed;
    }

    private async void OnCommandSpellButtonPressed()
    {
        if (CombatManager.Instance == null) return;
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault();
        if (player == null) return;

        var choiceContext = new BlockingPlayerChoiceContext();
        await FgoCommandSpellCmd.TryUseCommandSpell(choiceContext, player);
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

        foreach (Node child in node.GetChildren())
        foreach (var h in Find(child))
            yield return h;
    }
}