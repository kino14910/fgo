using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

public sealed partial class FgoVoidSeaBackground : Node
{
    private const string BackgroundScenePath = "res://Fgo/scenes/fgo_void_sea_bg.tscn";

    private const string VoidSeaLayerPath = "res://Fgo/scenes/fgo_void_sea_layer.tscn";

    private const float EnterFadeSeconds = 0.25f;

    private const float ExitFadeSeconds = 0.9f;

    private static FgoVoidSeaBackground? _instance;

    private static bool _reportedMissingInstance;

    private static bool _validated;

    private static bool _disabled;

    private static PackedScene? _backgroundScene;

    private static PackedScene? _layerScene;

    private Tween? _fade;

    private bool _lastInVoidSpace;

    private NCombatBackground? _voidSea;

    public static void Initialize()
    {
        ValidateAssets();

        var def = ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NCombatRoom, FgoVoidSeaBackground>(
                "void_sea_background",
                static _ => new FgoVoidSeaBackground(),
                static (_, node) => node.Bind(),
                new NodeAttachmentOptions
                {
                    Name = "FgoVoidSeaBackground",
                    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName
                });

        Entry.Logger.Info($"[Fgo] VoidSea: registered attachment '{def.Id}'");
    }

    private static void ValidateAssets()
    {
        _validated = true;
        try
        {
            _backgroundScene = LoadScene(BackgroundScenePath);
            _layerScene = LoadScene(VoidSeaLayerPath);
            if (_backgroundScene == null || _layerScene == null)
                throw new InvalidOperationException("background or layer scene failed to load");

            var root = _backgroundScene.Instantiate<Node>();
            try
            {
                if (root is not FgoVoidSeaBackgroundRoot ||
                    root.GetNodeOrNull<Control>("Layer_00") == null)
                    throw new InvalidOperationException(
                        $"scene root must carry {nameof(FgoVoidSeaBackgroundRoot)} and expose a 'Layer_00' Control");
            }
            finally
            {
                root.Free();
            }

            var layer = _layerScene.Instantiate<Node>();
            try
            {
                if (layer is not Control)
                    throw new InvalidOperationException("layer scene root must be a Control");
            }
            finally
            {
                layer.Free();
            }

            Entry.Logger.Info("[Fgo] VoidSea: assets validated.");
        }
        catch (Exception ex)
        {
            _disabled = true;
            Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: asset validation failed, feature disabled: {ex}");
#if DEBUG
            throw new InvalidOperationException("[Fgo] VoidSea: asset validation failed (DEBUG hard fail)", ex);
#endif
        }
    }

    public void Bind()
    {
        _instance = this;
        _reportedMissingInstance = false;
    }

    /// <remarks>每帧轮询、只在边沿动作：状态只由虚数脱出解除，挂在 OnPlay 上会换不回。</remarks>
    public static void Update(ICombatState? combat)
    {
        if (_disabled) return;

        var instance = _instance;
        if (instance == null || !IsInstanceValid(instance))
        {
            if (!_reportedMissingInstance)
            {
                _reportedMissingInstance = true;
                Entry.Logger.ErrorNoTrace(
                    "[Fgo] VoidSea: instance not bound - NCombatRoom _Ready attachment never ran " +
                    "(check for a '[NodeAttachment] Registered FGO_NODEATTACHMENT_VOID_SEA_BACKGROUND' log line).");
            }

            return;
        }

        var inVoidSpace = FgoVoidHand.IsInside(LocalContext.GetMe(combat));
        if (inVoidSpace == instance._lastInVoidSpace) return;

        Entry.Logger.Info($"[Fgo] VoidSea: edge -> inVoidSpace={inVoidSpace}");
        instance._lastInVoidSpace = inVoidSpace;
        if (inVoidSpace) instance.Enter();
        else instance.Exit();
    }

    /// <remarks>不重置 <c>_disabled</c>：资源不变量是会话级的，重置只会让每局重复报错。</remarks>
    public static void ResetForNewCombat()
    {
        var instance = _instance;
        if (instance == null || !IsInstanceValid(instance)) return;

        instance._lastInVoidSpace = false;
        instance.DiscardVoidSea();
    }

    private void Enter()
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;

        DiscardVoidSea();

        var voidSea = TryCreateBackground();
        if (voidSea == null) return;

        var vanilla = room.Background;

        // 必须挂进 %BgContainer 才能继承原版背景的缩放/裁剪/ZIndex 语境。
        var container = room.GetNodeOrNull<Control>("%BgContainer");
        if (container == null)
        {
            Entry.Logger.ErrorNoTrace("[Fgo] VoidSea: %BgContainer not found; cannot swap background.");
            voidSea.QueueFree();
            return;
        }

        voidSea.Modulate = new Color(1, 1, 1, 0);
        container.AddChild(voidSea);

        // 插在原版背景正后方：追加到末尾会盖住烘焙的房间特效（fire / square_specks）。
        var lastIndex = container.GetChildCount() - 1;
        var targetIndex = IsInstanceValid(vanilla) && vanilla.GetParent() == container
            ? vanilla.GetIndex() + 1
            : lastIndex;
        if (targetIndex < lastIndex)
            container.MoveChild(voidSea, targetIndex);

        Entry.Logger.Info($"[Fgo] VoidSea: background inserted at index {voidSea.GetIndex()} " +
                          $"(BgContainer has {container.GetChildCount()} children)");

        SetVanillaAlpha(vanilla, 1f);

        var fade = StartFade();
        if (fade == null)
        {
            voidSea.Modulate = Colors.White;
            SetVanillaAlpha(vanilla, 0f);
        }
        else
        {
            fade.TweenProperty(voidSea, "modulate:a", 1f, EnterFadeSeconds);
            fade.TweenProperty(vanilla, "modulate:a", 0f, EnterFadeSeconds);
        }

        _voidSea = voidSea;
    }

    private void Exit()
    {
        var room = NCombatRoom.Instance;
        var voidSea = _voidSea;

        // 淡出期间仍持有 _voidSea，否则再次进入会叠出第二份。
        if (room == null || voidSea == null || !IsInstanceValid(voidSea))
        {
            DiscardVoidSea();
            RestoreVanilla();
            return;
        }

        var vanilla = room.Background;
        var finished = voidSea;

        void Finish()
        {
            if (ReferenceEquals(_voidSea, finished)) _voidSea = null;
            RemoveAndFree(finished);
            if (IsInstanceValid(vanilla)) vanilla.Modulate = Colors.White;
        }

        var fade = StartFade();
        if (fade == null)
        {
            Finish();
            return;
        }

        fade.TweenProperty(voidSea, "modulate:a", 0f, ExitFadeSeconds);
        SetVanillaAlpha(vanilla, vanilla.Modulate.A);
        fade.TweenProperty(vanilla, "modulate:a", 1f, ExitFadeSeconds);

        fade.TweenCallback(Callable.From(Finish));
    }

    private static PackedScene? LoadScene(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: scene not found: {path}");
            return null;
        }

        return ResourceLoader.Load<PackedScene>(path);
    }

    private void RemoveAndFree(NCombatBackground node)
    {
        if (!IsInstanceValid(node)) return;
        var parent = node.GetParent();
        parent?.RemoveChild(node);
        node.QueueFree();
    }

    private NCombatBackground? TryCreateBackground()
    {
        if (_disabled) return null;
        if (!_validated) ValidateAssets();
        if (_disabled) return null;

        try
        {
            var scene = _backgroundScene;
            if (scene == null)
            {
                Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: background scene missing {BackgroundScenePath}");
                return null;
            }

            var root = scene.Instantiate<Node>() as NCombatBackground;
            if (root == null)
            {
                Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: Instantiate<{nameof(NCombatBackground)}> returned null " +
                                          $"- does the scene root carry the {nameof(FgoVoidSeaBackgroundRoot)} script?");
                return null;
            }

            if (!AddLayerInto(root, "Layer_00", VoidSeaLayerPath))
            {
                root.QueueFree();
                return null;
            }

            return root;
        }
        catch (Exception ex)
        {
            _disabled = true;
            Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: failed to build background, feature disabled: {ex}");
#if DEBUG
            throw;
#else
            return null;
#endif
        }
    }

    private static bool AddLayerInto(NCombatBackground root, string containerName, string layerPath)
    {
        var container = root.GetNodeOrNull<Control>(containerName);
        if (container == null)
        {
            Entry.Logger.ErrorNoTrace(
                $"[Fgo] VoidSea: background scene has no '{containerName}' node; layer cannot be attached.");
            return false;
        }

        _layerScene ??= LoadScene(layerPath);
        var layerScene = _layerScene;
        if (layerScene == null)
        {
            Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: layer scene missing {layerPath}");
            return false;
        }

        var layer = layerScene.Instantiate<Control>();
        layer.Visible = true;
        RitsuGodotTreeCompat.AddChildSafely(container, layer);
        return true;
    }

    private void DiscardVoidSea()
    {
        KillFade();

        if (_voidSea == null) return;
        RemoveAndFree(_voidSea);
        _voidSea = null;
    }

    private void RestoreVanilla()
    {
        var vanilla = NCombatRoom.Instance?.Background;
        if (vanilla == null || !IsInstanceValid(vanilla)) return;
        vanilla.Modulate = Colors.White;
        vanilla.Visible = true;
    }

    /// <remarks>Visible 必须保持 true，否则整棵子树不绘制、alpha 补间看不到效果。</remarks>
    private static void SetVanillaAlpha(NCombatBackground? vanilla, float alpha)
    {
        if (vanilla == null || !IsInstanceValid(vanilla)) return;
        vanilla.Visible = true;
        vanilla.Modulate = new Color(1, 1, 1, alpha);
    }

    private void KillFade()
    {
        if (_fade != null && _fade.IsValid()) _fade.Kill();
        _fade = null;
    }

    private Tween? StartFade()
    {
        KillFade();

        if (!IsInsideTree()) return null;
        return _fade = CreateTween();
    }
}