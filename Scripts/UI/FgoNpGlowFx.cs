using Godot;
using STS2RitsuLib;

namespace Fgo.Scripts.UI;

/// <remarks>参数全走 <see cref="FgoNpBar.NpGlowTuning" />，本类只持有节点、定位光心、转发强度。
/// 层级与鼠标过滤由 <see cref="FgoNpBar.BuildGlowFx" /> 及场景设置（否则整层盖住按钮会吃掉点击）。</remarks>
public sealed partial class FgoNpGlowFx : Control
{
    public const string ScenePath = "res://Fgo/scenes/fgo_np_glow_fx.tscn";

    public static readonly Vector2 FxSize = new(260f, 200f);

    public static readonly Vector2 OriginUv = new(0.5f, 0.5f);

    private static readonly StringName IntensityParam = "intensity";

    private static PackedScene? _scene;

    private ColorRect? _rays;
    private GpuParticles2D? _sparks;

    public ShaderMaterial? RayMaterial { get; private set; }

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

        RayMaterial?.SetShaderParameter("origin", OriginUv);
        RayMaterial?.SetShaderParameter("aspect", FxSize.X / FxSize.Y);
    }

    public void SetOriginLocal(Vector2 originLocal)
    {
        Position = originLocal - OriginUv * FxSize;
    }

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

        // 包络必须跟着参数走：越界粒子被 Godot 静默丢弃（无报错、无日志）。
        _sparks.VisibilityRect = ComputeEmberVisibilityRect(t);
    }

    /// <summary>0 = 熄灭，1 = 满亮度；&gt;1 用于点击爆发。</summary>
    public void SetIntensity(float value)
    {
        var v = Mathf.Max(value, 0f);

        RayMaterial?.SetShaderParameter(IntensityParam, v);

        if (_sparks == null)
            return;

        _sparks.AmountRatio = Mathf.Clamp(v, 0f, 1f);

        var emitting = v > 0.01f;

        if (_sparks.Emitting != emitting)
            _sparks.Emitting = emitting;
    }

    private static Rect2 ComputeEmberVisibilityRect(NpGlowTuning t)
    {
        var lifetime = Mathf.Max(t.EmberLifetime, 0.01f);

        var reach = t.EmberSpeedMax * lifetime + 0.5f * t.EmberAccelMax * lifetime * lifetime;
        var pad = 64f * t.EmberScaleMax + 24f;
        var half = reach + pad;

        return new Rect2(-half, -half, half * 2f, half * 2f);
    }

}