using Godot;

namespace Fgo.Scripts.UI;

public sealed partial class FgoVoidSeaLayer : Control
{
    private const string MaskNodePath = "TransitionMask";

    private const float CoverSeconds = 0.40f;

    private const float DissolveSeconds = 1.5f;

    private static readonly NodePath ThresholdParamPath = new("shader_parameter/threshold");

    private static readonly StringName ThresholdParamName = new("threshold");

    public override void _Ready()
    {
        PlayEnterTransition();
    }

    private void PlayEnterTransition()
    {
        var mask = GetNodeOrNull<ColorRect>(MaskNodePath);
        if (mask?.Material is not ShaderMaterial material) return;

        mask.Visible = true;
        material.SetShaderParameter(ThresholdParamName, 1f);

        var tween = CreateTween();
        tween.TweenInterval(CoverSeconds);
        tween.TweenProperty(material, ThresholdParamPath, 0f, DissolveSeconds)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() => mask.Visible = false));
    }
}