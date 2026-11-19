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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

/// <remarks>调参入口：改字段后 <see cref="FgoNpBar.ApplyGlowTuning" /> 生效。</remarks>
public sealed class NpGlowTuning
{
    public static readonly StringName OriginParam = "origin";
    public static readonly StringName AspectParam = "aspect";
    public static readonly StringName IntensityParam = "intensity";

    public static readonly StringName BreatheSpeedParam = "breathe_speed";
    public static readonly StringName BreatheAmountParam = "breathe_amount";

    public static readonly StringName CoreSizeParam = "core_size";
    public static readonly StringName CoreSoftParam = "core_soft";
    public static readonly StringName CoreGainParam = "core_gain";
    public static readonly StringName HaloSizeParam = "halo_size";
    public static readonly StringName HaloSoftParam = "halo_soft";
    public static readonly StringName HaloGainParam = "halo_gain";
    public static readonly StringName StreakGainParam = "streak_gain";

    public static readonly StringName RayCountParam = "ray_count";
    public static readonly StringName RayWidthParam = "ray_width";
    public static readonly StringName RaySharpParam = "ray_sharp";
    public static readonly StringName RayReachParam = "ray_reach";
    public static readonly StringName RayFalloffParam = "ray_falloff";
    public static readonly StringName RayGainParam = "ray_gain";
    public static readonly StringName RayShimmerParam = "ray_shimmer";

    public static readonly StringName StarSectorsParam = "star_sectors";
    public static readonly StringName StarRingsParam = "star_rings";
    public static readonly StringName StarDensityParam = "star_density";
    public static readonly StringName StarSizeParam = "star_size";
    public static readonly StringName StarArmParam = "star_arm";
    public static readonly StringName StarSharpParam = "star_sharp";
    public static readonly StringName StarReachParam = "star_reach";
    public static readonly StringName StarInnerParam = "star_inner";
    public static readonly StringName StarSpeedParam = "star_speed";
    public static readonly StringName StarGainParam = "star_gain";
    public static readonly StringName StarTwinkleParam = "star_twinkle";
    public static readonly StringName StarDimParam = "star_dim";

    public static readonly StringName RayColorParam = "ray_color";
    public static readonly StringName GlowColorParam = "glow_color";
    public static readonly StringName CoreColorParam = "core_color";
    public static readonly StringName SparkColorParam = "spark_color";

    public float BaseIntensity { get; set; } = 1.0f;

    public float IntensitySmooth { get; set; } = 7f;

    public float HoverBoost { get; set; } = 0.35f;

    public float BurstStrength { get; set; } = 1.4f;

    public float BurstDecay { get; set; } = 2.6f;

    public float ChargeGlow { get; set; } = 0f;

    public float ChargeRamp { get; set; } = 2.2f;

    public float BreatheSpeed { get; set; } = 0.3f;

    public float BreatheAmount { get; set; } = 0.18f;

    public float CoreSize { get; set; } = 0.07f;

    public float CoreSoft { get; set; } = 2.2f;

    public float CoreGain { get; set; } = 1f;

    public float HaloSize { get; set; } = 0.26f;

    public float HaloSoft { get; set; } = 2.8f;

    public float HaloGain { get; set; } = 0.55f;

    public float StreakGain { get; set; } = 0.3f;

    public float RayCount { get; set; } = 30f;

    public float RayWidth { get; set; } = 0.3f;

    public float RaySharp { get; set; } = 1.6f;

    public float RayReach { get; set; } = 0.3f;

    public float RayFalloff { get; set; } = 1.5f;

    public float RayGain { get; set; } = 0.42f;

    public float RayShimmer { get; set; } = 0.2f;

    public float StarSectors { get; set; } = 22f;

    public float StarRings { get; set; } = 7f;

    public float StarDensity { get; set; } = 0.42f;

    public float StarSize { get; set; } = 0.34f;

    public float StarArm { get; set; } = 2.6f;

    public float StarSharp { get; set; } = 1.5f;

    public float StarReach { get; set; } = 0.3f;

    public float StarInner { get; set; } = 0.05f;

    public float StarSpeed { get; set; } = 0.3f;

    public float StarGain { get; set; } = 0.55f;

    public float StarTwinkle { get; set; } = 0.45f;

    /// <summary>径向变暗量 0..1。着色器用 sqrt 保外圈可见亮度，直接换幂函数会让外圈星星消失。</summary>
    public float StarDim { get; set; } = 0.7f;

    public Color RayColor { get; set; } = new(1f, 0.7f, 0.21f);

    public Color GlowColor { get; set; } = new(1f, 0.78f, 0.3f);

    public Color CoreColor { get; set; } = new(1f, 0.965f, 0.855f);

    public Color SparkColor { get; set; } = new(1f, 0.93f, 0.7f);

    public int EmberAmount { get; set; } = 28;

    public float EmberGain { get; set; } = 1f;

    public float EmberLifetime { get; set; } = 2.2f;

    public float EmberSpeedMin { get; set; } = 3f;

    public float EmberSpeedMax { get; set; } = 12f;

    /// <summary>加速度上限（px/s²）。与 <see cref="EmberSpeedMax" /> 决定飞行距离，须与 <see cref="StarReach" /> 对齐。</summary>
    public float EmberAccelMax { get; set; } = 14f;

    public float EmberScaleMin { get; set; } = 0.04f;

    public float EmberScaleMax { get; set; } = 0.14f;
}

public sealed partial class FgoNpBar : Node
{
    private static readonly PackedScene? NpBarScene =
        GD.Load<PackedScene>("res://Fgo/scenes/fgo_np_bar.tscn");

    private NinePatchRect? _bar0;
    private NinePatchRect? _bar1;
    private NinePatchRect? _bar2;

    private TextureButton? _button;

    private NCreature? _creatureNode;

    private float _glowBurst;

    private bool _glowEnabled;

    private FgoNpGlowFx? _glowFx;

    private float _glowHover;

    private float _glowIntensity;

    private Vector2? _glowOrigin;

    private bool _hoverTipShown;
    private Control? _hpBarContainer;
    private MegaLabel? _hpLabel;
    private Label? _label;

    private bool _lastCanUse;

    private Vector2 _lastHpBarPosition;
    private Vector2 _lastHpBarSize;
    private Vector2 _lastHpLabelPosition;

    private int _lastNp = -1;
    private bool _lastSealed;
    private Control? _npBarRoot;
    private Player? _player;
    private FgoPlayerState? _subscribed;

    public NpGlowTuning NpGlowTuning { get; } = new();

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

        // npBar 被 NCreature 的 %Hitbox 压住，收不到 MouseEntered/Exited；悬停统一在 _Process 里按鼠标位置判定。
        BuildGlowFx();
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
        SyncGlowFxLayout();
        UpdateHoverTip();
        TickGlow(delta);

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

    /// <remarks>
    ///     绘制顺序按有效 z 升序、同 z 按树序。本层须与按钮同 z（0）—— +1 盖住按钮、-1 掉到立绘（z=0）之下；靠 <c>MoveChild</c> 到按钮之前让按钮后绘制压住它（按钮置前避免加色泛白）。
    ///     <c>_npBarRoot</c> 不能开 <c>clip_children</c>。
    /// </remarks>
    private void BuildGlowFx()
    {
        if (_npBarRoot == null || _button == null)
            return;

        var fx = FgoNpGlowFx.Create();

        if (fx == null)
            return;

        _npBarRoot.AddChild(fx);

        _npBarRoot.MoveChild(fx, _button.GetIndex());

        fx.ZAsRelative = true;
        fx.ZIndex = _button.ZIndex;

        _glowFx = fx;

        SyncGlowFxLayout();
        ApplyGlowTuning();
        SetGlowActive(false);
    }

    /// <remarks>每帧调用 + 变更检测：根尺寸改写后按钮局部位置要等 Godot 布局传递才更新，不能改尺寸当帧算一次。</remarks>
    private void SyncGlowFxLayout()
    {
        if (_glowFx == null || _button == null)
            return;

        var origin = _button.Position + _button.Size * 0.5f;

        if (_glowOrigin.HasValue && _glowOrigin.Value == origin)
            return;

        _glowOrigin = origin;
        _glowFx.SetOriginLocal(origin);
    }

    private void ApplyGlowTuning()
    {
        if (_glowFx == null)
            return;

        var t = NpGlowTuning;
        var mat = _glowFx.RayMaterial;

        if (mat == null)
            return;

        mat.SetShaderParameter(NpGlowTuning.OriginParam, new Vector2(0.5f, 0.5f));
        mat.SetShaderParameter(NpGlowTuning.AspectParam, FgoNpGlowFx.FxSize.X / FgoNpGlowFx.FxSize.Y);

        mat.SetShaderParameter(NpGlowTuning.BreatheSpeedParam, t.BreatheSpeed);
        mat.SetShaderParameter(NpGlowTuning.BreatheAmountParam, t.BreatheAmount);

        mat.SetShaderParameter(NpGlowTuning.CoreSizeParam, t.CoreSize);
        mat.SetShaderParameter(NpGlowTuning.CoreSoftParam, t.CoreSoft);
        mat.SetShaderParameter(NpGlowTuning.CoreGainParam, t.CoreGain);
        mat.SetShaderParameter(NpGlowTuning.HaloSizeParam, t.HaloSize);
        mat.SetShaderParameter(NpGlowTuning.HaloSoftParam, t.HaloSoft);
        mat.SetShaderParameter(NpGlowTuning.HaloGainParam, t.HaloGain);
        mat.SetShaderParameter(NpGlowTuning.StreakGainParam, t.StreakGain);

        mat.SetShaderParameter(NpGlowTuning.RayCountParam, t.RayCount);
        mat.SetShaderParameter(NpGlowTuning.RayWidthParam, t.RayWidth);
        mat.SetShaderParameter(NpGlowTuning.RaySharpParam, t.RaySharp);
        mat.SetShaderParameter(NpGlowTuning.RayReachParam, t.RayReach);
        mat.SetShaderParameter(NpGlowTuning.RayFalloffParam, t.RayFalloff);
        mat.SetShaderParameter(NpGlowTuning.RayGainParam, t.RayGain);
        mat.SetShaderParameter(NpGlowTuning.RayShimmerParam, t.RayShimmer);

        mat.SetShaderParameter(NpGlowTuning.StarSectorsParam, t.StarSectors);
        mat.SetShaderParameter(NpGlowTuning.StarRingsParam, t.StarRings);
        mat.SetShaderParameter(NpGlowTuning.StarDensityParam, t.StarDensity);
        mat.SetShaderParameter(NpGlowTuning.StarSizeParam, t.StarSize);
        mat.SetShaderParameter(NpGlowTuning.StarArmParam, t.StarArm);
        mat.SetShaderParameter(NpGlowTuning.StarSharpParam, t.StarSharp);
        mat.SetShaderParameter(NpGlowTuning.StarReachParam, t.StarReach);
        mat.SetShaderParameter(NpGlowTuning.StarInnerParam, t.StarInner);
        mat.SetShaderParameter(NpGlowTuning.StarSpeedParam, t.StarSpeed);
        mat.SetShaderParameter(NpGlowTuning.StarGainParam, t.StarGain);
        mat.SetShaderParameter(NpGlowTuning.StarTwinkleParam, t.StarTwinkle);
        mat.SetShaderParameter(NpGlowTuning.StarDimParam, t.StarDim);

        mat.SetShaderParameter(NpGlowTuning.RayColorParam, t.RayColor);
        mat.SetShaderParameter(NpGlowTuning.GlowColorParam, t.GlowColor);
        mat.SetShaderParameter(NpGlowTuning.CoreColorParam, t.CoreColor);
        mat.SetShaderParameter(NpGlowTuning.SparkColorParam, t.SparkColor);

        _glowFx.ApplyEmberTuning(t);
    }

    /// <remarks>指数缓动（帧率无关、带甩尾）。hover 自行按鼠标位置判定：npBar 被 Hitbox 压住，Godot 不派发 enter/leave。</remarks>
    private void TickGlow(double delta)
    {
        if (_glowFx == null)
            return;

        var t = NpGlowTuning;
        var dt = (float)delta;

        var hovering = false;

        if (_button != null && !_button.Disabled && _button.Visible)
            hovering = _button.GetGlobalRect().HasPoint(GetViewport().GetMousePosition());

        _glowHover = Mathf.MoveToward(_glowHover, hovering ? 1f : 0f, dt * 6f);

        if (_glowBurst > 0f)
            _glowBurst = Mathf.Max(0f, _glowBurst - t.BurstDecay * dt);

        var target = 0f;

        if (_glowEnabled)
        {
            target = t.BaseIntensity;

            if (t.ChargeGlow > 0f)
                target += t.ChargeGlow *
                          Mathf.Pow(Mathf.Clamp(_lastNp / 100f, 0f, 1f), t.ChargeRamp);

            target += t.HoverBoost * _glowHover + t.BurstStrength * _glowBurst;
        }

        var k = 1f - Mathf.Exp(-Mathf.Max(t.IntensitySmooth, 0.01f) * dt);
        _glowIntensity = Mathf.Lerp(_glowIntensity, target, k);

        _glowFx.Visible = _glowIntensity > 0.004f;
        _glowFx.SetIntensity(_glowIntensity * t.EmberGain);
    }

    private void SetGlowActive(bool active)
    {
        _glowEnabled = active;

        if (_glowFx == null)
            return;

        if (!active)
        {
            _glowBurst = 0f;
            _glowIntensity = 0f;
            _glowFx.SetIntensity(0f);
            _glowFx.Visible = false;
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
            // 远端玩家的按钮必须隐藏：点击后 hook action 因 owner 非本机永不入队、卡死。
            var isMe = LocalContext.IsMe(_player);
            var charged = isMe && np >= 100;
            var sealedNp = charged && _player!.Creature.HasPower<SealNpPower>();
            _lastSealed = sealedNp;

            var canUse = charged && !sealedNp;
            _button.Visible = charged;
            _button.Disabled = !canUse;

            // Disabled 只拦输入不改外观，置灰须手动改 Modulate。
            _button.Modulate = canUse ? Colors.White : StsColors.gray;

            SetGlowActive(canUse);

            // FTUE 只认 false→true 上升沿；传屏幕矩形（NP 条位置随分辨率/玩家变化）。
            if (canUse && !_lastCanUse)
                FgoFtue.MaybeShowNpBarFtue(_button.GetGlobalRect());

            _lastCanUse = canUse;
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

        _glowBurst = 1f;

        CallDeferred(nameof(DoNpButtonPressed));
    }

    private void OnNpButtonMouseEntered()
    {
        if (_button == null || _player == null)
            return;

        if (ShouldSuppressHoverTips())
            return;

        var tip = LocalContext.IsMe(_player) && _player!.Creature.HasPower<SealNpPower>()
            ? FgoHoverTipFactory.FromNpSealed()
            : FgoHoverTipFactory.FromNpButton();

        NHoverTipSet.CreateAndShow(_button, tip, HoverTipAlignment.Right);
    }

    private void OnNpButtonMouseExited()
    {
        if (_button == null)
            return;

        NHoverTipSet.Remove(_button);
    }

    /// <remarks>npBar 与 %Hitbox 重叠且在下层，鼠标总判给 Hitbox ⇒ 每帧自测指针进出，并压制会并存的生物提示。抑制条件见 <see cref="ShouldSuppressHoverTips" />。</remarks>
    private void UpdateHoverTip()
    {
        if (_npBarRoot == null)
            return;

        var mousePosition = GetViewport().GetMousePosition();
        var inBar = _npBarRoot.GetGlobalRect().HasPoint(mousePosition);

        if (!inBar && !_hoverTipShown)
            return;

        if (ShouldSuppressHoverTips())
        {
            if (_hoverTipShown)
            {
                _hoverTipShown = false;
                NHoverTipSet.Remove(_npBarRoot);
            }

            return;
        }

        if (inBar)
        {
            // 每帧压制：生物 OnFocus 订阅了 CombatStateChanged，状态变化会重弹提示。
            _creatureNode?.HideHoverTips();

            if (_hoverTipShown)
                return;

            _hoverTipShown = true;
            NHoverTipSet.CreateAndShow(
                _npBarRoot,
                FgoHoverTipFactory.FromNpBar(),
                HoverTipAlignment.Right
            );

            return;
        }

        _hoverTipShown = false;
        NHoverTipSet.Remove(_npBarRoot);

        // 移回生物身上时 Hitbox 仍 mouse-over、不会再发 MouseEntered，需手动补回提示。
        if (_creatureNode != null &&
            _creatureNode.Hitbox.GetGlobalRect().HasPoint(mousePosition))
            _creatureNode.ShowHoverTips(_creatureNode.Entity.HoverTips);
    }

    private static bool ShouldSuppressHoverTips()
    {
        if (NHoverTipSet.shouldBlockHoverTips)
            return true;

        if (NCombatRoom.Instance?.Ui.Hand.InCardPlay == true)
            return true;

        return ActiveScreenContext.Instance.GetCurrentScreen() is { } screen &&
               !ReferenceEquals(screen, NCombatRoom.Instance);
    }

    private void DoNpButtonPressed()
    {
        if (_player == null)
            return;

        if (!LocalContext.IsMe(_player))
            return;

        // 选牌须走托管动作队列：直接在 UI 事件里跑会让 host 等不到 SetChoiceContext、全游戏死锁。
        FgoNoblePhantasmCmd.Request(_player);
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