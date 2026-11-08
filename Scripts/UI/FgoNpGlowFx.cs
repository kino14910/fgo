using Godot;
using STS2RitsuLib;

namespace Fgo.Scripts.UI;

/// <summary>
///     NP 按钮的发光粒子层的场景宿主：只负责持有节点引用、定位光心、转发强度。
/// </summary>
/// <remarks>
///     节点结构（见场景）：<b>Rays</b>=ColorRect+np_gold_rays.gdshader（光核/辉光/拉丝/光芒/星星全在片元生成，无贴图）；
///     <b>Sparks</b>=GpuParticles2D（np_spark.png，补空气浮尘）。
///     <b>参数不在本类</b>：全部走 <see cref="FgoNpBar.NpGlowTuning" />+<see cref="FgoNpBar.ApplyGlowTuning" />，本类不存任何视觉默认值，避免场景/Export/代码三重真相。
///     <b>层级</b>：绘制在 <c>NpButton</c> 之下（按钮置前避免加色泛白），由 <see cref="FgoNpBar.BuildGlowFx" /> 设 z 与树序。
///     <b>鼠标</b>：根节点与 <c>Rays</c> 场景里已设 <c>mouse_filter = Ignore</c> 并此处兜底；<c>Sparks</c> 是 Node2D 不参与 GUI 命中，否则会吃点击。
/// </remarks>
public sealed partial class FgoNpGlowFx : Control
{
    public const string ScenePath = "res://Fgo/scenes/fgo_np_glow_fx.tscn";

    /// <summary>
    ///     特效层尺寸（像素）。与场景里根节点的 <c>offset_right/bottom</c> 对应。
    /// </summary>
    public static readonly Vector2 FxSize = new(260f, 200f);

    /// <summary>
    ///     固定为正中，<see cref="FgoNpBar" /> 每帧把本层对齐，从按钮中心向四周发射。
    /// </summary>
    public static readonly Vector2 OriginUv = new(0.5f, 0.5f);

    /// <summary>着色器里控制总亮度的 uniform 名。</summary>
    private static readonly StringName IntensityParam = "intensity";

    /// <summary>场景缓存。mod 自带场景不在房间预加载集里，自己缓存避免重复查盘与告警。</summary>
    private static PackedScene? _scene;

    private ColorRect? _rays;
    private GpuParticles2D? _sparks;

    /// <summary>解析光效的着色器材质，供 <see cref="FgoNpBar.ApplyGlowTuning" /> 写 uniform。</summary>
    public ShaderMaterial? RayMaterial { get; private set; }

    /// <summary>
    ///     实例化光效层场景。调用方负责 <c>AddChild</c> 与层级设置。
    /// </summary>
    public static FgoNpGlowFx? Create()
    {
        _scene ??= ResourceLoader.Load<PackedScene>(ScenePath);

        if (_scene == null)
        {
            Entry.Logger.ErrorNoTrace($"[Fgo] NpGlowFx: scene not found: {ScenePath}");
            return null;
        }

        var fx = _scene.Instantiate<FgoNpGlowFx>();

        if (fx == null)
            Entry.Logger.ErrorNoTrace(
                $"[Fgo] NpGlowFx: scene root is not {nameof(FgoNpGlowFx)} " +
                "(check the root node's script in the .tscn).");

        return fx;
    }

    public override void _Ready()
    {
        // 兜底：整层不参与命中测试，否则会挡住下方按钮的点击。
        MouseFilter = MouseFilterEnum.Ignore;
        Size = FxSize;

        _rays = GetNodeOrNull<ColorRect>("%Rays");
        _sparks = GetNodeOrNull<GpuParticles2D>("%Sparks");
        RayMaterial = _rays?.Material as ShaderMaterial;

        if (_rays != null)
            _rays.MouseFilter = MouseFilterEnum.Ignore;

        if (RayMaterial == null)
            Entry.Logger.ErrorNoTrace("[Fgo] NpGlowFx: %Rays node or its ShaderMaterial is missing.");

        if (_sparks == null)
            Entry.Logger.ErrorNoTrace("[Fgo] NpGlowFx: %Sparks node missing.");

        // 光心与 aspect 属于几何量，场景加载即可确定，不随调参变化。
        RayMaterial?.SetShaderParameter("origin", OriginUv);
        RayMaterial?.SetShaderParameter("aspect", FxSize.X / FxSize.Y);
    }

    public void SetOriginLocal(Vector2 originLocal)
    {
        Position = originLocal - OriginUv * FxSize;
    }

    /// <summary>
    ///     光尘粒子层参数。解析光效的 uniform 由 <see cref="FgoNpBar.ApplyGlowTuning" /> 写，
    ///     这里只处理 <see cref="GpuParticles2D" /> 侧的属性。
    /// </summary>
    public void ApplyEmberTuning(NpGlowTuning t)
    {
        if (_sparks == null)
            return;

        _sparks.Amount = Mathf.Max(t.EmberAmount, 1);
        _sparks.Lifetime = t.EmberLifetime;

        if (_sparks.ProcessMaterial is ParticleProcessMaterial pm)
        {
            pm.InitialVelocityMin = t.EmberSpeedMin;
            pm.InitialVelocityMax = t.EmberSpeedMax;
            pm.RadialAccelMin = t.EmberAccelMax * 0.35f;
            pm.RadialAccelMax = t.EmberAccelMax;
            pm.ScaleMin = t.EmberScaleMin;
            pm.ScaleMax = t.EmberScaleMax;
        }

        // 粒子活动包络必须跟着参数走，否则改大射程后粒子会飞出矩形被 Godot 静默丢弃
        // （越界粒子不报错、日志无痕，是"粒子完全不出现"的头号原因）。
        _sparks.VisibilityRect = ComputeEmberVisibilityRect(t);
    }

    /// <summary>
    ///     0 = 完全熄灭，1 = 满亮度（&gt;1 会让高光过曝，用于点击爆发）。
    /// </summary>
    /// <remarks>
    ///     解析光效走着色器 <c>intensity</c> uniform；光尘粒子走
    ///     <see cref="GpuParticles2D.AmountRatio" />，这样密度与亮度同步淡入淡出。
    /// </remarks>
    public void SetIntensity(float value)
    {
        var v = Mathf.Max(value, 0f);

        RayMaterial?.SetShaderParameter(IntensityParam, v);

        if (_sparks == null)
            return;

        // AmountRatio 上限为 1，强度再高也不会让粒子数变多，只会更亮。
        _sparks.AmountRatio = Mathf.Clamp(v, 0f, 1f);

        var emitting = v > 0.01f;

        if (_sparks.Emitting != emitting)
            _sparks.Emitting = emitting;
    }

    /// <summary>
    ///     按「初速 + 径向加速度 + 寿命」估算光尘的最大飞行距离，据此留出可见矩形。
    /// </summary>
    /// <remarks>
    ///     矩形是相对粒子节点自身原点的；粒子已位于光心（<see cref="OriginUv" /> × <see cref="FxSize" />），
    ///     所以这里直接以光心为圆心向外扩一个半径即可。
    /// </remarks>
    private static Rect2 ComputeEmberVisibilityRect(NpGlowTuning t)
    {
        var lifetime = Mathf.Max(t.EmberLifetime, 0.01f);

        // 径向匀加速上界：s = v·t + ½·a·t²。取最大初速与最大加速度（忽略阻尼，偏保守）。
        // 这里必须用 t.EmberAccelMax
        var reach = t.EmberSpeedMax * lifetime + 0.5f * t.EmberAccelMax * lifetime * lifetime;

        // 贴图是 64px，EmberScaleMax 是缩放系数，再留一点 turbulence 抖动余量。
        var pad = 64f * t.EmberScaleMax + 24f;
        var half = reach + pad;

        return new Rect2(-half, -half, half * 2f, half * 2f);
    }

}