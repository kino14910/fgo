using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace Fgo.Scripts.Utils;

/// <summary>
///     在角色头顶显示 "+xxNP" 浮动文本的 VFX 节点。
///     仿照原版 <c>NDamageNumVfx</c> 的动画行为，但使用自定义文本和金色 (D4AF37)。
///     纯代码创建，不依赖 .tscn 场景文件。
/// </summary>
public partial class FgoNpGainVfx : Label
{
    private static readonly Color NpColor = new("D4AF37");
    private static readonly Vector2 Gravity = new(0f, 2000f);

    private Vector2 _globalSpawnPosition;
    private Tween? _tween;
    private Vector2 _velocity;

    public override void _Ready()
    {
        // 文本外观
        AddThemeFontSizeOverride("font_size", 28);
        AddThemeColorOverride("font_color", NpColor);
        AddThemeColorOverride("font_outline_color", Colors.Black);
        AddThemeConstantOverride("outline_size", 4);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        // 定位
        GlobalPosition = _globalSpawnPosition;
        Scale = Vector2.One * 1.25f;
        RotationDegrees = Rng.Chaotic.NextFloat(-5f, 5f);

        // 初始向上飞出的速度
        _velocity = new Vector2(Rng.Chaotic.NextFloat(-100f, 100f), Rng.Chaotic.NextFloat(-800f, -700f));

        // 动画：淡入后淡出 + 缩放回归
        _tween = CreateTween().SetParallel();
        _tween.TweenProperty(this, "modulate", NpColor, 0.5)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        _tween.TweenProperty(this, "modulate:a", 0f, 2.0)
            .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        _tween.TweenProperty(this, "scale", Vector2.One, 1.2f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad)
            .From(Vector2.One * 2.5f);

        // 动画结束后自动释放
        TaskHelper.RunSafely(AnimAndFree());
    }

    public override void _Process(double delta)
    {
        var f = (float)delta;
        Position += _velocity * f;
        _velocity += Gravity * f;
    }

    public override void _ExitTree()
    {
        _tween?.Kill();
    }

    private async Task AnimAndFree()
    {
        if (_tween != null)
            await _tween.AwaitFinished(this);
        this.QueueFreeSafely();
    }

    /// <summary>
    ///     在指定玩家的角色头顶生成 "+xxNP" 浮动文本。
    ///     amount 为实际增加的 NP 值（正数）。
    /// </summary>
    public static Task Spawn(Player? player, int amount)
    {
        if (amount <= 0 || player == null) return Task.CompletedTask;

        var creature = player.Creature;
        var nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
        Vector2 globalPosition;

        if (nCreature is { IsInteractable: true })
        {
            globalPosition = nCreature.VfxSpawnPosition
                             + NCreature.PowerAppliedVfxPositionOffset
                             + new Vector2(Rng.Chaotic.NextFloat(-10f, 10f), Rng.Chaotic.NextFloat(-5f, 5f));
        }
        else
        {
            // 玩家角色节点不可用时，回退到屏幕左侧中部
            var size = ((SceneTree)Engine.GetMainLoop()).Root.GetViewport().GetVisibleRect().Size;
            globalPosition = size * new Vector2(0.25f, 0.5f);
        }

        var vfx = new FgoNpGainVfx
        {
            Text = $"+{amount}NP",
            _globalSpawnPosition = globalPosition
        };

        var container = NCombatRoom.Instance?.CombatVfxContainer;
        if (container != null)
            container.AddChildSafely(vfx);
        else
            NRun.Instance.GlobalUi.AddChildSafely(vfx);
        return Task.CompletedTask;
    }
}