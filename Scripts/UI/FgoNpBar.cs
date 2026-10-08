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

/// <summary>
///     NP 按钮金色发光粒子效果的全部可调参数。
/// </summary>
/// <remarks>
///     <para>
///         这是<b>唯一</b>的调参入口：改这里的字段后调用
///         <see cref="FgoNpBar.ApplyGlowTuning" />（或直接改数值，
///         <see cref="FgoNpBar.TickGlow" /> 每帧会自动重写）即实时生效。
///         场景 fgo_np_glow_fx.tscn 与着色器 np_gold_rays.gdshader 里的默认值只是美术基线，
///         运行期一律以本类为准。
///     </para>
///     <para>
///         强度链路：<c>enabled</c> → 蓄力度 → hover/burst 叠加 → 缓动 → 着色器 <c>intensity</c>。
///     </para>
/// </remarks>
public sealed class NpGlowTuning
{
    // ── 着色器 uniform 名 ────────────────────────────────────────────────
    // 集中在此，避免 SetShaderParameter 的字符串散落在业务代码里。

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

    // ── 强度与动效 ──────────────────────────────────────────────────────

    /// <summary>NP 满且可释放时的基础强度。</summary>
    public float BaseIntensity { get; set; } = 1.0f;

    /// <summary>强度缓动速度（每秒）。越大越"跟手"，越小越柔和。</summary>
    public float IntensitySmooth { get; set; } = 7f;

    /// <summary>悬停按钮时的强度加成。</summary>
    public float HoverBoost { get; set; } = 0.35f;

    /// <summary>点击瞬间叠加的爆发强度。</summary>
    public float BurstStrength { get; set; } = 1.4f;

    /// <summary>爆发余量的指数衰减速度（每秒）。</summary>
    public float BurstDecay { get; set; } = 2.6f;

    /// <summary>
    ///     NP 未满时的蓄力辉光上限。&gt;0 时按 <see cref="ChargeRamp" /> 在蓄力中缓慢点亮，
    ///     让玩家在充能过程中就看到反馈。
    /// </summary>
    public float ChargeGlow { get; set; } = 0f;

    /// <summary>蓄力辉光的映射指数。&gt;1 = 后段才亮（更含蓄），&lt;1 = 线性提早亮。</summary>
    public float ChargeRamp { get; set; } = 2.2f;

    // ── 呼吸 ──────────────────────────────────────────────────────────

    /// <summary>呼吸频率（Hz）。0.3 ≈ 3.3 秒一个周期。</summary>
    public float BreatheSpeed { get; set; } = 0.3f;

    /// <summary>呼吸深度。0.18 = 亮度在 100%~82% 之间起伏。</summary>
    public float BreatheAmount { get; set; } = 0.18f;

    // ── 光核 / 辉光 / 拉丝 ────────────────────────────────────────────

    /// <summary>光核半径（等比空间，1.0 = 层高 200px）。0.07 ≈ 14px，略小于按钮半高 20px。</summary>
    public float CoreSize { get; set; } = 0.07f;

    /// <summary>光核边缘锐度。</summary>
    public float CoreSoft { get; set; } = 2.2f;

    /// <summary>光核亮度增益。</summary>
    public float CoreGain { get; set; } = 1f;

    /// <summary>外层辉光半径（等比空间）。0.26 ≈ 52px，约 2.5 倍按钮半宽。</summary>
    public float HaloSize { get; set; } = 0.26f;

    /// <summary>辉光衰减指数。</summary>
    public float HaloSoft { get; set; } = 2.8f;

    /// <summary>辉光强度增益。</summary>
    public float HaloGain { get; set; } = 0.55f;

    /// <summary>横向拉丝强度。0 = 关闭。</summary>
    public float StreakGain { get; set; } = 0.3f;

    // ── 径向光芒 ──────────────────────────────────────────────────────

    /// <summary>光束数量。</summary>
    public float RayCount { get; set; } = 30f;

    /// <summary>单束角宽（0..0.5）。</summary>
    public float RayWidth { get; set; } = 0.3f;

    /// <summary>光束边缘硬度。</summary>
    public float RaySharp { get; set; } = 1.6f;

    /// <summary>光芒长度（等比空间）。0.30 ≈ 60px，刚好探出按钮轮廓一点。</summary>
    public float RayReach { get; set; } = 0.3f;

    /// <summary>沿半径的衰减指数。</summary>
    public float RayFalloff { get; set; } = 1.5f;

    /// <summary>光束强度增益。</summary>
    public float RayGain { get; set; } = 0.42f;

    /// <summary>每束光的独立闪烁幅度。</summary>
    public float RayShimmer { get; set; } = 0.2f;

    // ── 星星粒子 ──────────────────────────────────────────────────────

    /// <summary>角向分格数。建议 16~28；越大星星越密、越小。</summary>
    public float StarSectors { get; set; } = 22f;

    /// <summary>径向环数。建议 5~9；越大外扩层次越多。</summary>
    public float StarRings { get; set; } = 7f;

    /// <summary>槽位点亮比例 0..1，即"密度"主控。0.42 ≈ 22×7 格点亮 65 颗。</summary>
    public float StarDensity { get; set; } = 0.42f;

    /// <summary>星星尺寸（占所在格子的比例）。</summary>
    public float StarSize { get; set; } = 0.34f;

    /// <summary>星芒长度 / 星核半径。越大越"刺"，越小越"圆点"。</summary>
    public float StarArm { get; set; } = 2.6f;

    /// <summary>星芒对比度。</summary>
    public float StarSharp { get; set; } = 1.5f;

    /// <summary>粒子活动半径（等比空间）。应略大于 <see cref="RayReach" /> 让星星跑到光芒之外。</summary>
    public float StarReach { get; set; } = 0.3f;

    /// <summary>内圈淡出半径，避免中心糊成一坨白。</summary>
    public float StarInner { get; set; } = 0.05f;

    /// <summary>向外扩散速度（格/秒）。这是"速度"主控。</summary>
    public float StarSpeed { get; set; } = 0.3f;

    /// <summary>星星整体亮度增益。0.5 左右可让星星退居配角、不抢光核的白。</summary>
    public float StarGain { get; set; } = 0.55f;

    /// <summary>闪烁幅度。0.45 = 每颗星亮度在 55%~100% 之间跳。</summary>
    public float StarTwinkle { get; set; } = 0.45f;

    /// <summary>
    ///     径向变暗混合量 0..1：0 = 星星亮度不随半径衰减，1 = 向外明显变暗。
    ///     实现是 <c>mix(1, sqrt(1 - r/reach), star_dim)</c>，用 sqrt 是为了让外圈
    ///     仍保有可见亮度（直接用幂函数会让外圈星星集体消失）。
    /// </summary>
    public float StarDim { get; set; } = 0.7f;

    // ── 颜色 ──────────────────────────────────────────────────────────

    /// <summary>光芒 / 星星的外缘色（深金）。</summary>
    public Color RayColor { get; set; } = new(1f, 0.7f, 0.21f);

    /// <summary>辉光 / 拉丝色（中金）。</summary>
    public Color GlowColor { get; set; } = new(1f, 0.78f, 0.3f);

    /// <summary>光核色（近白）。</summary>
    public Color CoreColor { get; set; } = new(1f, 0.965f, 0.855f);

    /// <summary>星星高光色（预留，当前由 heat 混合主导）。</summary>
    public Color SparkColor { get; set; } = new(1f, 0.93f, 0.7f);

    // ── 光尘粒子层（GPUParticles2D）────────────────────────────────────

    /// <summary>光尘数量上限。</summary>
    public int EmberAmount { get; set; } = 28;

    /// <summary>光尘亮度比例，参与最终强度计算。</summary>
    public float EmberGain { get; set; } = 1f;

    /// <summary>光尘寿命（秒）。</summary>
    public float EmberLifetime { get; set; } = 2.2f;

    /// <summary>光尘初速下限。</summary>
    public float EmberSpeedMin { get; set; } = 3f;

    /// <summary>光尘初速上限。</summary>
    public float EmberSpeedMax { get; set; } = 12f;

    /// <summary>
    ///     光尘径向加速度上限（px/s²）。与 <see cref="EmberSpeedMax" /> 一起决定最远飞行距离
    ///     <c>v·t + ½·a·t²</c>，应与 <see cref="StarReach" /> 对齐，否则光尘会拖到光芒之外。
    /// </summary>
    public float EmberAccelMax { get; set; } = 14f;

    /// <summary>光尘尺寸下限（粒子缩放）。</summary>
    public float EmberScaleMin { get; set; } = 0.04f;

    /// <summary>光尘尺寸上限（粒子缩放）。</summary>
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

    /// <summary>
    ///     金色发光粒子层，绘制在 <see cref="_button" /> 及其所有子节点<b>之下</b>（按钮保持在最前）。
    /// </summary>
    /// <remarks>
    ///     层级与鼠标的完整推导见 <see cref="BuildGlowFx" />。
    /// </remarks>
    private FgoNpGlowFx? _glowFx;

    /// <summary>上一次写进光效层的光心位置（按钮中心，npBar 局部坐标）。</summary>
    private Vector2? _glowOrigin;

    /// <summary>上一次的光效点亮状态，用于只在边沿打一条诊断日志。</summary>
    private bool _lastGlowOn;

    /// <summary>光效的实时强度（已做缓动），最终写进着色器的 <c>intensity</c>。</summary>
    private float _glowIntensity;

    /// <summary>悬停按钮时的强度加成，缓动跟随。</summary>
    private float _glowHover;

    /// <summary>点击瞬间的爆发余量，按 <see cref="NpGlowTuning.BurstDecay" /> 指数衰减。</summary>
    private float _glowBurst;

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

    /// <summary>
    ///     上一次的 <c>canUse</c>（NP 满且未封印）状态。
    /// </summary>
    /// <remarks>
    ///     与 <see cref="_lastGlowOn" /> 分开：那个只用于光效的边沿诊断日志，
    ///     语义是「光效是否点亮」；这个用于 FTUE 触发的 false→true 上升沿。
    ///     两者恰好同值，但用途与生命周期不同，合并会让「光效诊断日志」被 FTUE 逻辑污染。
    /// </remarks>
    private bool _lastCanUse;
    private Control? _npBarRoot;
    private Player? _player;
    private FgoPlayerState? _subscribed;

    /// <summary>
    ///     发光粒子效果的全部可调参数。改完字段后调用 <see cref="ApplyGlowTuning" /> 生效。
    /// </summary>
    public NpGlowTuning NpGlowTuning { get; } = new();

    /// <summary>NP 满且可释放时是否点亮光效。由 <see cref="OnNpChanged" /> 维护。</summary>
    private bool _glowEnabled;

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

    /// <summary>
    ///     挂上发光粒子层，使其绘制在 <c>NpButton</c> 之下（按钮保持最前），同时仍高于生物立绘。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Godot 的 CanvasItem 绘制顺序是「先按有效 <c>z_index</c> 升序、同 z 再按树序」，
    ///         有效 z = 父节点有效 z + 自身 <c>z_index</c>（<c>z_as_relative</c> 开启时）。
    ///     </para>
    ///     <para>
    ///         <b>为什么不能简单取 <c>_button.ZIndex - 1</c>：</b>
    ///         本层挂在 <c>NCreatureStateDisplay</c> 子树里，而立绘
    ///         <c>NCreature.Visuals</c> 是 state display 的<b>兄弟</b>且
    ///         <c>z_index = 0</c>（原版在 NCreature.cs 里用 <c>MoveChild(Visuals, 0)</c>
    ///         把它强制到树序最前）。本机玩家 state display 的有效 z 也是 0，于是：
    ///     </para>
    ///     <list type="bullet">
    ///         <item><c>fx.ZIndex = +1</c> → 有效 z = 1，<b>高于</b>立绘(0) ⇒ 可见（但会盖住按钮）</item>
    ///         <item><c>fx.ZIndex = -1</c> → 有效 z = -1，<b>低于</b>立绘(0) ⇒ 被立绘整个盖住 ⇒ 看不见</item>
    ///     </list>
    ///     <para>
    ///         所以正确做法是让本层与按钮<b>同 z（0），靠树序决胜</b>：本层仍是 state display
    ///         子树内的一员，有效 z 与立绘同为 0，而它在树序上远后于立绘（Visuals 是 Creature 的
    ///         第 0 个子节点），因此依旧画在立绘之上；同时在 npBar 内部把它 <c>MoveChild</c> 到
    ///         按钮之前，按钮后绘制、稳稳压住它。
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <b>z_index</b> —— 与按钮取<b>相同</b>值（<c>_button.ZIndex</c>，通常 0）。
    ///             不能 +1 也不能 -1：前者盖住按钮，后者掉到立绘之下。
    ///         </item>
    ///         <item>
    ///             <b>树序</b> —— <c>MoveChild</c> 到按钮<b>之前</b>。同 z 时树序是唯一决胜条件，
    ///             这是「按钮在前」真正生效的地方。
    ///         </item>
    ///         <item>
    ///             <b>鼠标</b> —— 根节点与 <c>Rays</c> 都设了 <c>mouse_filter = Ignore</c>，
    ///             <see cref="FgoNpGlowFx" /> 的 <c>_Ready</c> 再兜底设一次；
    ///             <c>Sparks</c> 是 <c>GPUParticles2D</c>（Node2D），本身不参与 GUI 命中测试。
    ///             本层整块覆盖按钮区域，必须 Ignore 才不会吃掉点击。
    ///         </item>
    ///     </list>
    ///     <para>
    ///         <b>为什么按钮要在前面</b>：按钮贴图 <c>np_max</c> 是暖橙色，与金色光效同色系。
    ///         粒子层压在上面时，加色叠加会让贴图和 <c>NpLabel</c> 的数字一起泛白、读不清。
    ///         让按钮处于最前，中心最亮的光核被按钮本体盖住，光芒与星星只从按钮轮廓外露出，
    ///         既保留「从按钮中心向外发射」的观感，又保证按钮可读。
    ///     </para>
    ///     <para>
    ///         粒子层尺寸（260×200）远大于 <c>_npBarRoot</c>，会溢出到兄弟节点之上。
    ///         这是「向外发射」所必需的效果，故确认 <c>_npBarRoot</c> 未开启 <c>clip_children</c>
    ///         （见 fgo_np_bar.tscn），不会被裁掉。
    ///     </para>
    /// </remarks>
    private void BuildGlowFx()
    {
        if (_npBarRoot == null || _button == null)
            return;

        var fx = FgoNpGlowFx.Create();

        if (fx == null)
            return;

        _npBarRoot.AddChild(fx);

        // 放到按钮之前：与按钮同 z 时，树序是唯一决胜条件，这一步才是「按钮在前」真正生效的地方。
        _npBarRoot.MoveChild(fx, _button.GetIndex());

        // 关键：与按钮【同 z】，靠树序决胜。
        // 不能 -1（会掉到 NCreature.Visuals 立绘之下被整个盖住），也不能 +1（会盖住按钮）。
        fx.ZAsRelative = true;
        fx.ZIndex = _button.ZIndex;

        _glowFx = fx;

        SyncGlowFxLayout();
        ApplyGlowTuning();
        SetGlowActive(false);

        // 诊断：确认绘制次序（父节点名 / 子节点索引 / 相对 z），用于排查粒子层被遮挡。
        var order = new System.Text.StringBuilder();
        for (var i = 0; i < _npBarRoot.GetChildCount(); i++)
        {
            var child = _npBarRoot.GetChild(i);
            order.Append($"[{i}]{child.Name}(z={((child as CanvasItem)?.ZIndex ?? 0)}) ");
        }

        Entry.Logger.Info($"[Fgo] NpGlowFx tree after insert: {order}");

        // 诊断：打印【有效 z】与立绘的相对关系。遮挡问题只看相对 z 看不出来 ——
        // 立绘 Visuals 是 state display 的兄弟（z=0，原版用 MoveChild(Visuals, 0) 强制到树序最前），
        // 本层若有效 z < 0 就会掉到它下面被整个盖住，而画面上完全看不出原因。
        var parent = GetParent();
        var parentZ = (parent as CanvasItem)?.ZIndex ?? 0;
        var siblings = new System.Text.StringBuilder();

        if (parent != null)
        {
            for (var i = 0; i < parent.GetChildCount(); i++)
            {
                var child = parent.GetChild(i);
                siblings.Append($"{i}:{child.Name}(z={(child as CanvasItem)?.ZIndex ?? 0}) ");
            }
        }

        // fx 的有效 z = 沿链累加；这里只关心「相对 parent 的 z」是否与立绘同级。
        Entry.Logger.Info(
            $"[Fgo] NpGlowFx z: fx={fx.ZIndex} npBar={_npBarRoot.ZIndex} button={_button.ZIndex} " +
            $"parent={parent?.Name}(z={parentZ}) | parentChildren=[{siblings}]");
    }

    /// <summary>
    ///     把光效层的光心对准按钮中心。
    /// </summary>
    /// <remarks>
    ///     每帧调用但自带变更检测: 按钮是 <c>_npBarRoot</c> 的锚定子节点，根节点尺寸被
    ///     <see cref="SyncWithHealthBar" /> 改写成血条尺寸后，按钮的局部位置要等 Godot
    ///     的布局传递才会更新，所以不能在改尺寸的同一帧里算一次就完事。
    /// </remarks>
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

    /// <summary>
    ///     把 <see cref="NpGlowTuning" /> 里的全部参数写进光效层（着色器 uniform + 粒子节点）。
    /// </summary>
    /// <remarks>
    ///     改任意字段后再调用一次即可实时生效，无需重载场景或重启战斗。
    /// </remarks>
    private void ApplyGlowTuning()
    {
        if (_glowFx == null)
            return;

        var t = NpGlowTuning;
        var mat = _glowFx.RayMaterial;

        if (mat == null)
            return;

        // 坐标系：光心固定在光效层正中，aspect 由层尺寸决定。
        mat.SetShaderParameter(NpGlowTuning.OriginParam, new Vector2(0.5f, 0.5f));
        mat.SetShaderParameter(NpGlowTuning.AspectParam, FgoNpGlowFx.FxSize.X / FgoNpGlowFx.FxSize.Y);

        // 呼吸
        mat.SetShaderParameter(NpGlowTuning.BreatheSpeedParam, t.BreatheSpeed);
        mat.SetShaderParameter(NpGlowTuning.BreatheAmountParam, t.BreatheAmount);

        // 光核 / 辉光 / 拉丝
        mat.SetShaderParameter(NpGlowTuning.CoreSizeParam, t.CoreSize);
        mat.SetShaderParameter(NpGlowTuning.CoreSoftParam, t.CoreSoft);
        mat.SetShaderParameter(NpGlowTuning.CoreGainParam, t.CoreGain);
        mat.SetShaderParameter(NpGlowTuning.HaloSizeParam, t.HaloSize);
        mat.SetShaderParameter(NpGlowTuning.HaloSoftParam, t.HaloSoft);
        mat.SetShaderParameter(NpGlowTuning.HaloGainParam, t.HaloGain);
        mat.SetShaderParameter(NpGlowTuning.StreakGainParam, t.StreakGain);

        // 径向光芒
        mat.SetShaderParameter(NpGlowTuning.RayCountParam, t.RayCount);
        mat.SetShaderParameter(NpGlowTuning.RayWidthParam, t.RayWidth);
        mat.SetShaderParameter(NpGlowTuning.RaySharpParam, t.RaySharp);
        mat.SetShaderParameter(NpGlowTuning.RayReachParam, t.RayReach);
        mat.SetShaderParameter(NpGlowTuning.RayFalloffParam, t.RayFalloff);
        mat.SetShaderParameter(NpGlowTuning.RayGainParam, t.RayGain);
        mat.SetShaderParameter(NpGlowTuning.RayShimmerParam, t.RayShimmer);

        // 星星粒子：尺寸 / 速度 / 密度三项主控
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

        // 颜色
        mat.SetShaderParameter(NpGlowTuning.RayColorParam, t.RayColor);
        mat.SetShaderParameter(NpGlowTuning.GlowColorParam, t.GlowColor);
        mat.SetShaderParameter(NpGlowTuning.CoreColorParam, t.CoreColor);
        mat.SetShaderParameter(NpGlowTuning.SparkColorParam, t.SparkColor);

        // 光尘粒子层
        _glowFx.ApplyEmberTuning(t);
    }

    /// <summary>
    ///     每帧把「动画状态」换算成光效强度，并写进着色器。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         强度链路（<c>enabled</c> 由 <see cref="OnNpChanged" /> 维护）：
    ///         <code>
    ///         target = enabled ? BaseIntensity + ChargeGlow * pow(np/100, ChargeRamp)
    ///                          + HoverBoost * hover + BurstStrength * burst
    ///                         : 0
    ///         intensity = lerp(intensity, target, 1 - exp(-IntensitySmooth * dt))
    ///         </code>
    ///         指数缓动而非线性插值：帧率无关，且天然带一点"甩尾"，与呼吸/闪烁叠加后不显生硬。
    ///     </para>
    ///     <para>
    ///         <b>hover</b> 自行用鼠标位置判定，不用 <c>MouseEntered</c>：
    ///         npBar 被生物的 Hitbox 压在下方，Godot 永远不会派发 enter/leave 事件
    ///         （同 <see cref="UpdateHoverTip" /> 里说明的问题）。
    ///     </para>
    /// </remarks>
    private void TickGlow(double delta)
    {
        if (_glowFx == null)
            return;

        var t = NpGlowTuning;
        var dt = (float)delta;

        // 悬停：按钮矩形命中测试。按钮不可用时不算悬停。
        var hovering = false;

        if (_button != null && !_button.Disabled && _button.Visible)
            hovering = _button.GetGlobalRect().HasPoint(GetViewport().GetMousePosition());

        _glowHover = Mathf.MoveToward(_glowHover, hovering ? 1f : 0f, dt * 6f);

        // 点击爆发：按下瞬间抬起，之后按 BurstDecay 指数回落。
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

        // 帧率无关的指数趋近。
        var k = 1f - Mathf.Exp(-Mathf.Max(t.IntensitySmooth, 0.01f) * dt);
        _glowIntensity = Mathf.Lerp(_glowIntensity, target, k);

        // 极低强度时整层隐藏，既省绘制也避免残留微光。
        _glowFx.Visible = _glowIntensity > 0.004f;
        _glowFx.SetIntensity(_glowIntensity * t.EmberGain);
    }

    /// <summary>
    ///     开关光效（不改动强度目标值之外的任何状态）。
    /// </summary>
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

            // 光效只在「NP 满且可释放」时点亮：未满与封印态（灰按钮）都保持熄灭，
            // 与按钮自身的可见性/置灰状态保持一致。强度本身由 TickGlow 每帧缓动，
            // 这里只切目标开关，因此不会出现瞬间跳变。
            SetGlowActive(canUse);

            if (canUse != _lastGlowOn)
            {
                _lastGlowOn = canUse;
                _glowFx?.DumpRuntimeState($"canUse={canUse},np={np}");
            }

            // FTUE：宝具条「刚变得可用」的那一刻弹一次教学。
            // 只认 false→true 上升沿，避免每次 NpChanged 变更都重复尝试弹窗。
            // 首见标记由 FgoFtue 用 Global 存档槽维护，弹过一次之后这里就是空转。
            //
            // 传按钮的屏幕矩形：NP 条跟着 HP 条走，位置随分辨率/玩家位置变化，
            // 弹窗要贴着按钮弹、箭头要指着按钮，都得运行时算而不是场景里写死。
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

        // 点击瞬间给光效一次爆发，随后由 TickGlow 按 BurstDecay 回落。
        _glowBurst = 1f;

        CallDeferred(nameof(DoNpButtonPressed));
    }

    private void OnNpButtonMouseEntered()
    {
        if (_button == null || _player == null)
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
                FgoHoverTipFactory.FromNpBar(),
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