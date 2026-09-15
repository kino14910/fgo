using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
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

    private NCreature? _creatureNode;
    private bool _hoverTipShown;
    private Control? _hpBarContainer;
    private MegaLabel? _hpLabel;
    private Label? _label;

    private Vector2 _lastHpBarPosition;
    private Vector2 _lastHpBarSize;
    private Vector2 _lastHpLabelPosition;

    private int _lastNp = -1;
    private bool _lastSealed;
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

        _creatureNode = creatureNode;

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

        // npBar 是 NCreatureStateDisplay 的子节点，而 NCreature 的 %Hitbox 是 state display 之后的
        // 兄弟节点（绘制在上层、命中测试先于整棵 state display 子树），且 npBar 的矩形恰好落在
        // Hitbox 矩形内，所以 Godot 永远把鼠标判给 Hitbox，npBar 收不到 MouseEntered/MouseExited。
        // 条区域与按钮的悬停统一在 _Process 中通过鼠标位置检测实现。
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

        var sealedNow = _player != null && LocalContext.IsMe(_player) && _player.Creature.HasPower<SealNpPower>();
        if (resources.Np != _lastNp || sealedNow != _lastSealed)
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
            var isMe = LocalContext.IsMe(_player);
            var charged = isMe && np >= 100;
            var sealedNp = charged && _player!.Creature.HasPower<SealNpPower>();
            _lastSealed = sealedNp;

            var canUse = charged && !sealedNp;
            _button.Visible = charged;
            _button.Disabled = !canUse;

            // Godot 的 BaseButton.Disabled 只拦截输入、不改变外观（TextureButton 未设
            // texture_disabled 时仍画 texture_normal），所以置灰必须手动改 Modulate——
            // 与本体 NTickbox / NRunModifierTickbox 在 OnDisable 里改 Modulate 的做法一致。
            _button.Modulate = canUse ? Colors.White : StsColors.gray;
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
        if (_button == null || _player == null)
            return;

        var tip = LocalContext.IsMe(_player) && _player!.Creature.HasPower<SealNpPower>()
            ? FgoHoverTipHelper.CreateNpSealedHoverTip()
            : FgoHoverTipHelper.CreateNpButtonHoverTip();

        NHoverTipSet.CreateAndShow(_button, tip, HoverTipAlignment.Right);
    }

    private void OnNpButtonMouseExited()
    {
        if (_button == null)
            return;

        NHoverTipSet.Remove(_button);
    }

    /// <summary>
    ///     条区域的悬停提示：npBar 与 %Hitbox 矩形重叠且位于其下层，命中测试始终把鼠标判给 Hitbox，
    ///     因此这里每帧自行判定指针进出，并同步压制生物的悬停提示（两者 owner 不同，会并存）。
    /// </summary>
    private void UpdateHoverTip()
    {
        if (_npBarRoot == null)
            return;

        var mousePosition = GetViewport().GetMousePosition();

        if (_npBarRoot.GetGlobalRect().HasPoint(mousePosition))
        {
            // Hitbox 一直处于 mouse-over，指针移进 npBar 不会让它触发 MouseExited；
            // 又因 OnFocus 订阅了 CombatStateChanged，战斗中状态变化还会让生物重弹提示，
            // 所以每帧压制而非只在进入的那一帧调用（Remove 对不存在的 owner 是空操作）。
            _creatureNode?.HideHoverTips();

            if (_hoverTipShown)
                return;

            _hoverTipShown = true;
            NHoverTipSet.CreateAndShow(
                _npBarRoot,
                FgoHoverTipHelper.CreateNpBarHoverTip(),
                HoverTipAlignment.Right
            );

            return;
        }

        if (!_hoverTipShown)
            return;

        _hoverTipShown = false;
        NHoverTipSet.Remove(_npBarRoot);

        // 指针只是从 npBar 移回生物身上时，Hitbox 仍是 mouse-over、Godot 不会再发 MouseEntered，
        // 需在此补回生物的悬停提示；移出 Hitbox 则交给 Godot 自己处理。
        if (_creatureNode != null &&
            _creatureNode.Hitbox.GetGlobalRect().HasPoint(mousePosition))
            _creatureNode.ShowHoverTips(_creatureNode.Entity.HoverTips);
    }

    private void DoNpButtonPressed()
    {
        if (_player == null)
            return;

        // 双保险：仅本机玩家可发起（按钮对远端玩家本就隐藏，见 OnNpChanged）。
        if (!LocalContext.IsMe(_player))
            return;

        // 选牌作为托管网络动作走官方动作队列（药水 UsePotionAction 模式），
        // 在所有 peer 上执行；此前直接在 UI 事件里跑选牌会让 host 侧
        // 永远等不到 SetChoiceContext，动作队列死锁、全游戏卡死。
        FgoNoblePhantasmCmd.Request();
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