using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

public sealed partial class FgoNpBar : Node
{
    private static readonly PackedScene? NpBarScene =
        GD.Load<PackedScene>("res://Fgo/scenes/fgo_np_bar.tscn");

    private NinePatchRect? _bar0;
    private NinePatchRect? _bar1;
    private NinePatchRect? _bar2;

    private TextureButton? _button;
    private bool _hoverTipShown;
    private Control? _hpBarContainer;
    private MegaLabel? _hpLabel;
    private Label? _label;

    private Vector2 _lastHpBarPosition;
    private Vector2 _lastHpBarSize;
    private Vector2 _lastHpLabelPosition;

    private int _lastNp = -1;
    private Control? _npBarRoot;

    private Player? _player;
    private FgoPlayerState? _subscribed;

    public static void Initialize()
    {
        ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NCreatureStateDisplay, FgoNpBar>(
                "np_bar_controller",
                static _ => new FgoNpBar(),
                new NodeAttachmentOptions
                {
                    Name = "FgoNpBar",
                    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName
                });
    }

    public override void _Ready()
    {
        if (GetParent() is not NCreatureStateDisplay stateDisplay)
        {
            SetProcess(false);
            return;
        }

        var creatureNode = FindParentOfType<NCreature>();

        if (creatureNode?.Entity.IsPlayer != true)
        {
            SetProcess(false);
            return;
        }

        var player = creatureNode.Entity.Player;
        if (player?.Character is not FgoCharacter)
        {
            SetProcess(false);
            return;
        }

        _player = player;

        var healthBar = stateDisplay.GetNodeOrNull<NHealthBar>("%HealthBar");

        if (healthBar?.HpBarContainer == null)
        {
            SetProcess(false);
            return;
        }

        _hpBarContainer = healthBar.HpBarContainer;
        _hpLabel = healthBar.GetNodeOrNull<MegaLabel>("%HpLabel");

        if (_hpLabel == null || NpBarScene == null)
        {
            SetProcess(false);
            return;
        }

        var npBar = NpBarScene.Instantiate<Control>();
        stateDisplay.AddChild(npBar);

        _npBarRoot = npBar;

        _bar0 = _npBarRoot.FindChild("Bar0", true, false) as NinePatchRect;
        _bar1 = _npBarRoot.FindChild("Bar1", true, false) as NinePatchRect;
        _bar2 = _npBarRoot.FindChild("Bar2", true, false) as NinePatchRect;

        _label = _npBarRoot.FindChild("NpLabel", true, false) as Label;
        _button = _npBarRoot.FindChild("NpButton", true, false) as TextureButton;

        if (_button != null)
        {
            _button.Pressed += OnNpButtonPressed;
            _button.MouseEntered += OnNpButtonMouseEntered;
            _button.MouseExited += OnNpButtonMouseExited;
        }

        // 根节点 mouse_filter 为 IGNORE，Godot 的 MouseEntered/MouseExited 不会触发，
        // 因此条区域的悬停提示统一在 _Process 中通过鼠标位置检测实现。
        InitializeBarLayout();
        SyncWithHealthBar();

        if (_player != null)
            TrySubscribe(FgoBattleHooks.Get(_player));
    }

    public override void _Process(double delta)
    {
        if (_npBarRoot == null ||
            _hpBarContainer == null ||
            _hpLabel == null)
            return;

        SyncWithHealthBar();
        UpdateHoverTip();

        if (!CombatManager.Instance.IsInProgress &&
            !CombatManager.Instance.IsStarting)
            return;

        if (_player == null)
            return;

        var resources = FgoBattleHooks.Get(_player);

        if (_subscribed != resources)
            TrySubscribe(resources);

        if (resources.Np != _lastNp)
            OnNpChanged(resources.Np);
    }

    public override void _ExitTree()
    {
        if (_button != null)
        {
            _button.Pressed -= OnNpButtonPressed;
            NHoverTipSet.Remove(_button);
        }

        if (_npBarRoot != null)
            NHoverTipSet.Remove(_npBarRoot);

        if (_subscribed != null)
        {
            _subscribed.NpChanged -= OnNpChanged;
            _subscribed = null;
        }
    }

    private void SyncWithHealthBar()
    {
        if (_npBarRoot == null ||
            _hpBarContainer == null ||
            _hpLabel == null)
            return;

        var hpBarPosition = _hpBarContainer.GlobalPosition;
        var hpBarSize = _hpBarContainer.Size;
        var hpLabelPosition = _hpLabel.GlobalPosition;

        if (hpBarPosition != _lastHpBarPosition ||
            hpBarSize != _lastHpBarSize ||
            hpLabelPosition != _lastHpLabelPosition)
        {
            _npBarRoot.GlobalPosition = new Vector2(
                hpBarPosition.X,
                hpLabelPosition.Y - _npBarRoot.Size.Y - 2f
            );

            _npBarRoot.Size = hpBarSize;

            _lastHpBarPosition = hpBarPosition;
            _lastHpBarSize = hpBarSize;
            _lastHpLabelPosition = hpLabelPosition;

            UpdateBarWidths();
        }
    }

    private void InitializeBarLayout()
    {
        if (_npBarRoot == null ||
            _bar0 == null ||
            _bar1 == null ||
            _bar2 == null)
            return;

        var foregroundContainer =
            _npBarRoot.GetNodeOrNull<Control>("NpForegroundContainer");

        if (foregroundContainer == null)
            return;

        var width = foregroundContainer.Size.X;

        if (width <= 0f)
            return;

        UpdateBar(_bar0, 0f, width);
        UpdateBar(_bar1, 0f, width);
        UpdateBar(_bar2, 0f, width);
    }

    private void TrySubscribe(FgoPlayerState resources)
    {
        if (_subscribed != null)
            _subscribed.NpChanged -= OnNpChanged;

        _subscribed = resources;
        _subscribed.NpChanged += OnNpChanged;

        OnNpChanged(resources.Np);
    }

    private void OnNpChanged(int np)
    {
        _lastNp = np;

        UpdateBarWidths();

        if (_label != null)
            _label.Text = np.ToString();

        if (_button != null)
        {
            // 多人下仅本机玩家可点击：远端玩家的 creature 也会在本机渲染，
            // 其按钮必须隐藏，否则点击后 hook action 因 owner 非本机而永不入队，造成卡死。
            var canUse = np >= 100 && LocalContext.IsMe(_player);
            _button.Visible = canUse;
            _button.Disabled = !canUse;
        }
    }

    private void UpdateBarWidths()
    {
        if (_npBarRoot == null)
            return;

        var foregroundContainer =
            _npBarRoot.GetNodeOrNull<Control>("NpForegroundContainer");

        if (foregroundContainer == null)
            return;

        var maxBarWidth = foregroundContainer.Size.X;

        if (maxBarWidth <= 0f)
            return;

        UpdateBar(
            _bar0,
            Mathf.Clamp(_lastNp, 0, 100) / 100f,
            maxBarWidth
        );

        UpdateBar(
            _bar1,
            Mathf.Clamp(_lastNp - 100, 0, 100) / 100f,
            maxBarWidth
        );

        UpdateBar(
            _bar2,
            Mathf.Clamp(_lastNp - 200, 0, 100) / 100f,
            maxBarWidth
        );
    }

    private static void UpdateBar(
        NinePatchRect? bar,
        float fill,
        float maxWidth)
    {
        if (bar == null)
            return;

        bar.Visible = fill > 0f;
        bar.Size = new Vector2(maxWidth * fill, bar.Size.Y);
    }

    private void OnNpButtonPressed()
    {
        if (_player == null)
            return;

        CallDeferred(nameof(DoNpButtonPressed));
    }

    private void OnNpButtonMouseEntered()
    {
        if (_button == null)
            return;

        NHoverTipSet.CreateAndShow(
            _button,
            FgoHoverTipHelper.CreateNpBarHoverTip(),
            HoverTipAlignment.Right
        );
    }

    private void OnNpButtonMouseExited()
    {
        if (_button == null)
            return;

        NHoverTipSet.Remove(_button);
    }

    /// <summary>
    ///     条区域（含 NpButton）的悬停提示：根节点 mouse_filter 为 IGNORE，
    ///     无法使用 MouseEntered 信号，改为每帧检测鼠标是否位于条矩形内。
    /// </summary>
    private void UpdateHoverTip()
    {
        if (_npBarRoot == null)
            return;

        var inside = _npBarRoot.GetGlobalRect().HasPoint(GetViewport().GetMousePosition());

        if (inside && !_hoverTipShown)
        {
            _hoverTipShown = true;
            NHoverTipSet.CreateAndShow(
                _npBarRoot,
                FgoHoverTipHelper.CreateNpBarHoverTip(),
                HoverTipAlignment.Right
            );
        }
        else if (!inside && _hoverTipShown)
        {
            _hoverTipShown = false;
            NHoverTipSet.Remove(_npBarRoot);
        }
    }

    private async void DoNpButtonPressed()
    {
        if (_player == null)
            return;

        // 双保险：仅本机玩家可发起宝具选牌。多人下点击非本机玩家的按钮
        // 会导致 HookPlayerChoiceContext 卡死在 hook action 入队等待。
        if (!LocalContext.IsMe(_player))
            return;

        // UI 按钮触发的选牌: 用 HookPlayerChoiceContext 把选择作为一个新 GameAction 排入
        // 本玩家队列，多人下其他玩家的队列不会被阻塞（官方文档: 战斗场景应优先 Hook 而非 Blocking）。
        // 第二个参数必须传 LocalContext.NetId（官方写法），而非 _player.NetId：
        // 它是"本地玩家 Id"，SignalPlayerChoiceBegun 靠它决定是否为本机入队 hook action。
        var choiceContext = new HookPlayerChoiceContext(
            _player, LocalContext.NetId!.Value, GameActionType.Combat);
        await choiceContext.AssignTaskAndWaitForPauseOrCompletion(
            FgoNoblePhantasmCmd.TryChooseNoblePhantasm(choiceContext, _player));
    }

    private T? FindParentOfType<T>()
        where T : Node
    {
        var current = GetParent();

        while (current != null)
        {
            if (current is T target)
                return target;

            current = current.GetParent();
        }

        return null;
    }
}