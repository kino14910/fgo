using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

/// <remarks>只摘自己加的节点：<c>%BgContainer</c> 还烘焙了房间特效，清空会永久删掉。不碰 <c>NCombatRoom.Background</c>（setter 私有且会重复 AddChild），只做视觉叠加。挂 <c>NCombatRoom</c> 而非 <c>NCombatUi</c>（后者的 <c>_isDebugHidden</c> 跨战斗残留会把背景设透明）。</remarks>
public sealed partial class FgoVoidSeaBackground : Node
{
    private const string BackgroundScenePath = "res://Fgo/scenes/fgo_void_sea_bg.tscn";

    private const string VoidSeaLayerPath = "res://Fgo/scenes/fgo_void_sea_layer.tscn";

    private const float EnterFadeSeconds = 0.25f;

    private const float ExitFadeSeconds = 0.9f;

    private static FgoVoidSeaBackground? _instance;

    private static bool _reportedMissingInstance;

    private static PackedScene? _backgroundScene;

    private static PackedScene? _layerScene;

    private Tween? _fade;

    private bool _lastInVoidSpace;

    private NCombatBackground? _voidSea;

    public static void Initialize()
    {
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

    /// <remarks>初始化期一次性校验资源，把"场景根挂错脚本"从运行期 InvalidCastException 变成显式失败。
    /// DEBUG 下直接抛出（开发期不可能漏）；RELEASE 下记录并熔断此功能，不影响 mod 其余部分。</remarks>
    public void Bind()
    {
        _instance = this;
        _reportedMissingInstance = false;
    }

    /// <remarks>每帧轮询、只在边沿动作：状态只由〔虚数脱出〕解除，挂在 OnPlay 上会换不回。只认本地玩家。</remarks>
    public static void Update(ICombatState? combat)
    {
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
        var targetIndex = vanilla != null && IsInstanceValid(vanilla) && vanilla.GetParent() == container
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
            // 建不出补间（节点不在树内）就直接落最终态，至少保证背景换上。
            voidSea.Modulate = Colors.White;
            SetVanillaAlpha(vanilla, 0f);
        }
        else
        {
            fade.TweenProperty(voidSea, "modulate:a", 1f, EnterFadeSeconds);
            if (vanilla != null) fade.TweenProperty(vanilla, "modulate:a", 0f, EnterFadeSeconds);
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
            if (vanilla != null && IsInstanceValid(vanilla)) vanilla.Modulate = Colors.White;
        }

        var fade = StartFade();
        if (fade == null)
        {
            Finish();
            return;
        }

        fade.TweenProperty(voidSea, "modulate:a", 0f, ExitFadeSeconds);
        if (vanilla != null)
        {
            SetVanillaAlpha(vanilla, vanilla.Modulate.A);
            fade.TweenProperty(vanilla, "modulate:a", 1f, ExitFadeSeconds);
        }

        fade.TweenCallback(Callable.From(Finish));
    }

    /// <remarks>用 <c>ResourceLoader.Load</c> 而非 <c>PreloadManager.Cache.GetScene</c>：后者对 mod 场景永远未命中、每次打 "Asset not cached" 告警。</remarks>
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

    /// <remarks>资源不变量在 <see cref="ValidateAssets"/> 校验；此处按 <c>Node</c> 实例化以避开
    /// <c>Instantiate&lt;NCombatBackground&gt;</c> 的 <c>InvalidCastException</c>。首次运行期失败即熔断，不再重试。</remarks>
    private NCombatBackground? TryCreateBackground()
    {
        try
        {
            _backgroundScene ??= LoadScene(BackgroundScenePath);
            var scene = _backgroundScene;
            if (scene == null)
            {
                Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: background scene missing {BackgroundScenePath}");
                return null;
            }

            var root = scene.Instantiate<NCombatBackground>();
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
            Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: failed to build background: {ex}");
            return null;
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

    /// <remarks><c>Visible</c> 必须保持 true，否则整棵子树不绘制、alpha 补间看不到效果。</remarks>
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