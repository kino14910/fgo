using Fgo.Scripts.Fields;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

/// <summary>
///     〔虚数空间〕的战斗背景替换: 进入时把战场背景换成虚数之海，退出时淡回原背景。
/// </summary>
/// <remarks>
///     <para>
///         <b>不要动原版 <c>%BgContainer</c> 里原有的子节点。</b>
///         它是 <c>combat_room.tscn</c> 里一个**所有战斗共用**的节点，除了本场战斗的背景
///         （<c>room.Background</c>，由 <c>SetUpBackground</c> 挂进去）之外，场景本身还烘焙了
///         <c>SpineSprite</c>、带 shader 的 <c>TextureRect</c>、<c>fire</c>、<c>square_specks</c> 等一堆特效节点。
///         所以「清空 BgContainer 再把新背景塞进去」是错的——那会把这批共用的房间特效永久删掉。
///         正确做法: 只把自己加进去的那个节点摘掉，原版背景节点只做显隐/透明度控制。
///     </para>
///     <para>
///         <b>不要碰 <c>NCombatRoom.Background</c>，也绝不调 <c>SetUpBackground</c>。</b>
///         它的 setter 是私有的；更要紧的是 <c>SetUpBackground</c> 见到 <c>Background != null</c>
///         会打一条 "Tried to set up background twice!" 警告并重复 <c>AddChild</c>。
///         游戏只在 <c>OnCombatSetUp</c> 里判空读它一次，战斗开始后无人再读，
///         所以要「换背景」只需在 <c>%BgContainer</c> 上做视觉层面的叠加与显隐。
///     </para>
///     <para>
///         <b>不要把节点挂在 <c>NCombatUi</c> 下。</b>
///         <c>NCombatUi._Ready()</c> 末尾若静态字段 <c>_isDebugHidden</c> 为真，会把所有非 <c>Hand</c>
///         的直属 <c>Control</c> 一律设成 <c>Modulate = Colors.Transparent</c>，而该字段**跨战斗残留**，
///         一旦触发就会让之后每场战斗的背景永久不可见。这里挂在 <c>NCombatRoom</c> 自己身上，规避这条路径。
///     </para>
/// </remarks>
public sealed partial class FgoVoidSeaBackground : Node
{
    /// <summary>背景场景路径。场景自带 <c>Layer_00</c> 图层容器。</summary>
    private const string BackgroundScenePath = "res://Fgo/scenes/fgo_void_sea_bg.tscn";

    /// <summary>虚数之海图层场景。必须内含 <c>Visual</c> 与 <c>PhobiaModeVisual</c> 两个直属子节点。</summary>
    private const string VoidSeaLayerPath = "res://Fgo/scenes/fgo_void_sea_layer.tscn";

    /// <summary>淡入淡出时长（秒）。</summary>
    private const float FadeSeconds = 0.6f;

    private static FgoVoidSeaBackground? _instance;

    /// <summary>「实例未绑定」的告警只发一次，避免每帧刷屏。</summary>
    private static bool _reportedMissingInstance;

    /// <summary>当前插入的背景节点。淡出过程中仍持有它，直到收尾回调真正移除，期间重复进入不会叠出第二份。</summary>
    private NCombatBackground? _voidSea;

    /// <summary>上一次观察到的「是否处于虚数空间」，用于做边沿检测。</summary>
    private bool _lastInVoidSpace;

    /// <summary>当前补间；切换方向时先杀掉，避免两条补间互相抢 alpha。</summary>
    private Tween? _fade;

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

    public void Bind()
    {
        _instance = this;
        _reportedMissingInstance = false;
    }

    /// <summary>
    ///     按当前场地状态推进入/出〔虚数空间〕的背景切换。每帧幂等，只在边沿处动作。
    /// </summary>
    /// <remarks>
    ///     刻意做成轮询而不是只挂在 <c>GreatVoidSeaBattle.OnPlay</c> 上: 场地还可能被
    ///     〔虚数脱出〕主动移除、或层数自然归零，那些路径都不会再走到大冲角的 OnPlay，
    ///     只在那里触发就会留下永远换不回来、后续战斗还把背景压住的问题。
    ///     轮询点与 <c>FgoGlobalHud.SyncVoidSpaceHand</c> 同源（同一帧、同一份 <c>ICombatState</c>），
    ///     手牌与背景的进出时机因此天然一致。
    /// </remarks>
    public static void Update(ICombatState? combat)
    {
        var instance = _instance;
        if (instance == null || !IsInstanceValid(instance))
        {
            // 每帧都会走到这里，只报一次，避免刷屏。
            if (!_reportedMissingInstance)
            {
                _reportedMissingInstance = true;
                Entry.Logger.ErrorNoTrace(
                    "[Fgo] VoidSea: instance not bound - NCombatRoom _Ready attachment never ran " +
                    "(check for a '[NodeAttachment] Registered FGO_NODEATTACHMENT_VOID_SEA_BACKGROUND' log line).");
            }

            return;
        }

        var inVoidSpace = FgoField.Has(combat, FgoFieldId.ImaginarySpace);
        if (inVoidSpace == instance._lastInVoidSpace) return;

        Entry.Logger.Info($"[Fgo] VoidSea: edge -> inVoidSpace={inVoidSpace}");
        instance._lastInVoidSpace = inVoidSpace;
        if (inVoidSpace) instance.Enter();
        else instance.Exit();
    }

    /// <summary>新战斗开始: 上一场若停在虚数空间，复用实例时必须重置边沿状态并清掉残留节点。</summary>
    /// <remarks>
    ///     上一场战斗的 <c>NCombatRoom</c> 及其 <c>Background</c> 都已随房间销毁，
    ///     所以这里只需要拆掉自己那份节点、把边沿状态归零；原版背景的 alpha 无需恢复
    ///     （新房间的背景是全新实例，<c>modulate</c> 天生就是 <c>Colors.White</c>）。
    /// </remarks>
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

        // 新背景挂进 %BgContainer，与原版背景同一层:
        // 这样才继承到同一套缩放/裁剪/ZIndex 语境。挂在 NCombatRoom 自己身下会跑到
        // SceneContainer 的兄弟层去，与 BgContainer 的 scale/anchor 对不上。
        var container = room.GetNodeOrNull<Control>("%BgContainer");
        if (container == null)
        {
            Entry.Logger.ErrorNoTrace("[Fgo] VoidSea: %BgContainer not found; cannot swap background.");
            voidSea.QueueFree();
            return;
        }

        // 新背景先透明入场，再补间到不透明；原版背景同时反向淡出，两者叠加才是真正的交叉淡入。
        voidSea.Modulate = new Color(1, 1, 1, 0);
        container.AddChild(voidSea);

        // 插到原版背景**正后方**，而不是简单追加到末尾:
        // BgContainer 里除原版背景外还烘焙了 fire / square_specks 等房间特效，
        // 追加到末尾会把虚数之海盖到这些特效之上、把房间氛围糊掉。
        // 贴在原版背景之后，既压住原版背景，又让特效继续叠在最上层。
        var lastIndex = container.GetChildCount() - 1;
        var targetIndex = vanilla != null && IsInstanceValid(vanilla) && vanilla.GetParent() == container
            ? vanilla.GetIndex() + 1
            : lastIndex;
        if (targetIndex < lastIndex)
            container.MoveChild(voidSea, targetIndex);

        Entry.Logger.Info($"[Fgo] VoidSea: background inserted at index {voidSea.GetIndex()} " +
                          $"(BgContainer has {container.GetChildCount()} children)");

        // 原版背景必须先保证 visible=true 才能让 alpha 生效——Godot 的 CanvasItem 只要 Visible=false
        // 就整棵子树不绘制，那时再怎么补间 modulate 也看不见。
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
            fade.TweenProperty(voidSea, "modulate:a", 1f, FadeSeconds);
            if (vanilla != null) fade.TweenProperty(vanilla, "modulate:a", 0f, FadeSeconds);
        }

        _voidSea = voidSea;
    }

    private void Exit()
    {
        var room = NCombatRoom.Instance;
        var voidSea = _voidSea;

        // 淡出期间 _voidSea 仍持有节点（否则这段时间里再次进入会叠出第二份）。
        // 这里只把「正在淡出」的意图记在 _voidSea 上，由收尾回调负责真正移除。
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
            // 建不出补间就直接收尾：虚数之海立刻移除、原版背景恢复满不透明。
            Finish();
            return;
        }

        fade.TweenProperty(voidSea, "modulate:a", 0f, FadeSeconds);
        if (vanilla != null)
        {
            // 原版背景从当前 alpha 补到不透明。Enter 时它已被补到 0 附近，
            // 若整段淡入尚未跑完就被打断，这里从"当前值"续上即可，不必强行重置到 0。
            SetVanillaAlpha(vanilla, vanilla.Modulate.A);
            fade.TweenProperty(vanilla, "modulate:a", 1f, FadeSeconds);
        }

        fade.TweenCallback(Callable.From(Finish));
    }

    /// <summary>背景场景与图层场景各缓存一份（mod 自带的场景不在房间预加载集里，自己缓存避免重复查盘与告警）。</summary>
    private static PackedScene? _backgroundScene;

    private static PackedScene? _layerScene;

    /// <summary>
    ///     取 mod 自带场景。用 <c>ResourceLoader.Load</c> 而非 <c>PreloadManager.Cache.GetScene</c>:
    ///     后者对本 mod 的场景永远未命中，会 fallback 并**每次**打一条
    ///     <c>"Asset not cached: res://Fgo/..."</c> 告警。这些场景不在房间的预加载集里，
    ///     与其产生噪音，不如自己缓存一份。
    /// </summary>
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

    /// <summary>
    ///     组装虚数之海背景: 实例化背景场景并填入图层。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         走「实例化背景场景」而不是原版 <c>NCombatBackground.Create</c>，是因为需要自定义场景路径。
    ///     </para>
    ///     <para>
    ///         <b>背景场景的根节点必须挂了 <see cref="FgoVoidSeaBackgroundRoot" />（继承 <c>NCombatBackground</c>）。</b>
    ///         这里用的 <c>Instantiate&lt;NCombatBackground&gt;</c> 重载要求场景根本身就是
    ///         <c>NCombatBackground</c> 的实例；根若是裸 <c>Control</c>，会抛 <c>InvalidCastException</c>。
    ///         另外场景自带的 <c>Layer_00</c> 容器名必须与查找键一致，否则拿不到容器、图层挂不上去。
    ///     </para>
    /// </remarks>
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

            var root = scene.Instantiate<NCombatBackground>(PackedScene.GenEditState.Disabled);
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
            // 打完整异常（含类型与堆栈）。此前只打 Message，把 InvalidCastException 这类
            // "根节点类型不对"的错误压成一句无信息的文案，导致定位困难。
            Entry.Logger.ErrorNoTrace($"[Fgo] VoidSea: failed to build background: {ex}");
            return null;
        }
    }

    /// <summary>
    ///     把图层场景塞进背景场景自带的同名容器。返回是否成功。
    /// </summary>
    /// <remarks>
    ///     等价于原版 <c>NCombatBackground.AddLayer</c>（同样"查同名子节点 → 实例化图层 → 挂上去"），
    ///     区别是容器名由调用方与背景场景约定好，不必再走一次会抛异常的查找。
    ///     容器缺失时原版会抛 <c>InvalidOperationException</c>；这里改为返回 false 并交调用方放弃，
    ///     因为"挂了个没有任何图层的背景"只会静默地显示成空白，比直接失败更难查。
    /// </remarks>
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

        var layer = layerScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
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

    /// <summary>
    ///     原版背景只是淡出，不移出场景树: 它的 <c>visible</c> 必须保持 true，否则整棵子树不绘制、
    ///     modulate 的 alpha 补间将完全看不到效果。
    /// </summary>
    private static void SetVanillaAlpha(NCombatBackground? vanilla, float alpha)
    {
        if (vanilla == null || !IsInstanceValid(vanilla)) return;
        vanilla.Visible = true;
        vanilla.Modulate = new Color(1, 1, 1, alpha);
    }

    /// <summary>杀掉当前补间，不做别的。</summary>
    /// <remarks>
    ///     只杀、不建。Godot 的 <c>CreateTween()</c> 会立刻开始跑那条补间，
    ///     若建出来却没加任何 Tweener，引擎会报 "Tween ... started with no Tweeners."
    ///     ⇒ 所以"只是取消旧补间"的路径（如 <see cref="DiscardVoidSea" />）只能走这里。
    /// </remarks>
    private void KillFade()
    {
        if (_fade != null && _fade.IsValid()) _fade.Kill();
        _fade = null;
    }

    /// <summary>
    ///     杀掉旧补间并起一条新的。返回 null 表示当前无法建补间（节点不在场景树内）。
    /// </summary>
    private Tween? StartFade()
    {
        KillFade();

        if (!IsInsideTree()) return null;
        return _fade = CreateTween();
    }
}
