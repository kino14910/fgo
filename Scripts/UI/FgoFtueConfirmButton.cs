using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace Fgo.Scripts.UI;

/// <summary>
///     mod 自有的 FTUE 确认按钮，等价于原版 <c>NFtueConfirmButton</c>。
/// </summary>
public sealed partial class FgoFtueConfirmButton : NButton
{
    private const int FontMinSize = 12;

    private const int FontMaxSize = 20;
    private static readonly StringName VParam = "v";
    private ShaderMaterial? _hsv;

    private TextureRect? _outline;
    private Tween? _outlineTween;
    private Tween? _scaleTween;

    protected override string[] Hotkeys { get; } = [MegaInput.select];

    public override void _Ready()
    {
        ConnectSignals();

        _outline = GetNodeOrNull<TextureRect>("Outline");
        _hsv = Material as ShaderMaterial;

        var label = GetNodeOrNull<Label>("%Label");
        if (label != null)
            FgoFtueText.SetAutoSizeText(
                label,
                new LocString("ftues", "CONFIRM_BUTTON").GetRawText(),
                FontMinSize,
                FontMaxSize
            );

        _outlineTween = CreateTween().SetLoops();
        _outlineTween.TweenProperty(_outline, "modulate:a", 1f, 0.6);
        _outlineTween.TweenProperty(_outline, "modulate:a", 0.25f, 0.6);
    }

    protected override void OnFocus()
    {
        base.OnFocus();

        _outlineTween?.Kill();
        if (_outline != null) _outline.Modulate = StsColors.gold;

        _scaleTween?.Kill();
        _scaleTween = CreateTween();
        _scaleTween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.05)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
        _scaleTween.TweenMethod(
            Callable.From<float>(UpdateShaderV),
            _hsv?.GetShaderParameter(VParam) ?? 1f,
            1.4f,
            0.05
        ).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
    }

    protected override void OnUnfocus()
    {
        base.OnUnfocus();

        _scaleTween?.Kill();
        _scaleTween = CreateTween();
        _scaleTween.TweenProperty(this, "scale", Vector2.One, 0.5)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
        _scaleTween.TweenMethod(
            Callable.From<float>(UpdateShaderV),
            _hsv?.GetShaderParameter(VParam) ?? 1f,
            1f,
            0.5
        ).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);

        _outlineTween?.Kill();
        _outlineTween = CreateTween().SetLoops();
        _outlineTween.TweenProperty(_outline, "modulate:a", 0.25f, 0.6);
        _outlineTween.TweenProperty(_outline, "modulate:a", 1f, 0.6)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
    }

    private void UpdateShaderV(float value)
    {
        _hsv?.SetShaderParameter(VParam, value);
    }
}