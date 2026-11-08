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
///     <b>不动 <c>%BgContainer</c> 原有子节点</b>：它所有战斗共用，除本场背景（<c>room.Background</c>）外还烘焙了
///     SpineSprite / fire / square_specks 等房间特效，清空会永久删掉这批共用特效；只摘自己加的节点。
///     <b>不碰 <c>NCombatRoom.Background</c> / 不调 <c>SetUpBackground</c></b>：setter 私有，且见非 null 会打
///     "set up background twice" 并重复 AddChild；战斗开始后无人再读，换背景只需在 %BgContainer 上做视觉叠加与显隐。
///     <b>不挂 <c>NCombatUi</c> 下</b>：其 <c>_Ready</c> 末尾若 <c>_isDebugHidden</c> 为真会把非 Hand 直属 Control 设透明，
///     且该字段跨战斗残留，会让之后每场背景永久不可见；故挂在 <c>NCombatRoom</c> 自身规避。
/// </remarks>
public sealed partial class FgoVoidSeaBackground : Node
{
    private const string BackgroundScenePath = "res://Fgo/scenes/fgo_void_sea_bg.tscn";

    /// <summary>虚数之海图层场景。必须内含 <c>Visual</c> 与 <c>PhobiaModeVisual</c> 两个直属子节点。</summary>
    private const string VoidSeaLayerPath = "res://Fgo/scenes/fgo_void_sea_layer.tscn";

    /// <summary>
    ///     进入时把虚数之海本体推到不透明所需的时长（秒）。
    ///     <para>
    ///         <b>刻意压得很短。</b>画面的「切入」现在由 <c>TransitionMask</c> 的遮罩溶解负责，
    ///         这里的 modulate 只负责在遮罩完全盖住屏幕的期间把底图备好；
    ///         若这里也拖到 1s+，就会和遮罩溶解叠成「两层同时淡入」，
    ///         看起来又像原来的直接交叉淡入，遮罩的溶解过程反而被冲淡。
    ///         0.25s 足够在不透过遮罩的前提下完成，且远小于
    ///         <c>FgoVoidSeaLayer.CoverSeconds = 0.40</c>，不会漏出底图。
    ///     </para>
    /// </summary>
    private const float EnterFadeSeconds = 0.25f;

    /// <summary>
    ///     离开时淡出并拆除的时长（秒）。
    ///     <para>
    ///         离场没有遮罩接管（图层会整体淡掉），所以保留一段可读的交叉淡出，
    ///         取 0.9s：比原来的 0.6s 慢一半，肉眼能看清是「虚数之海退去」而不是闪断。
    ///     </para>
    /// </summary>
    private const float ExitFadeSeconds = 0.9f;

    private static FgoVoidSeaBackground? _instance;

    /// <summary>「实例未绑定」的告警只发一次，避免每帧刷屏。</summary>
    private static bool _reportedMissingInstance;

    /// <summary>背景场景与图层场景各缓存一份（mod 自带的场景不在房间预加载集里，自己缓存避免重复查盘与告警）。</summary>
    private static PackedScene? _backgroundScene;

    private static PackedScene? _layerScene;

    /// <summary>当前补间；切换方向时先杀掉，避免两条补间互相抢 alpha。</summary>
    private Tween? _fade;

    private bool _lastInVoidSpace;

    /// <summary>当前插入的背景节点。淡出过程中仍持有它，直到收尾回调真正移除，期间重复进入不会叠出第二份。</summary>
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

    public void Bind()
    {
        _instance = this;
        _reportedMissingInstance = false;
    }

    /// <summary>
    ///     按本地玩家的〔虚数空间〕状态推进入/出背景切换。每帧幂等，只在边沿处动作。
    /// </summary>
    /// <remarks>
    ///     刻意轮询而非只挂 <c>GreatVoidSeaBattle.OnPlay</c>：状态只由〔虚数脱出〕解除，
    ///     只在那触发会留下换不回、后续战斗压住背景的问题。
    ///     轮询点与 <c>FgoGlobalHud.SyncVoidSpaceHand</c> 同源（同帧同 <c>ICombatState</c>），手牌与背景进出时机天然一致。
    ///     联机时只认本地玩家：别人进入虚数空间不该换掉自己的战场背景。
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
            // 这里只把底图「备好」到不透明；真正的画面切入由图层内 TransitionMask 的
            // 遮罩溶解完成，所以这段刻意很短（EnterFadeSeconds），避免和溶解叠成两重淡入。
            fade.TweenProperty(voidSea, "modulate:a", 1f, EnterFadeSeconds);
            if (vanilla != null) fade.TweenProperty(vanilla, "modulate:a", 0f, EnterFadeSeconds);
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

        fade.TweenProperty(voidSea, "modulate:a", 0f, ExitFadeSeconds);
        if (vanilla != null)
        {
            // 原版背景从当前 alpha 补到不透明。Enter 时它已被补到 0 附近，
            // 若整段淡入尚未跑完就被打断，这里从"当前值"续上即可，不必强行重置到 0。
            SetVanillaAlpha(vanilla, vanilla.Modulate.A);
            fade.TweenProperty(vanilla, "modulate:a", 1f, ExitFadeSeconds);
        }

        fade.TweenCallback(Callable.From(Finish));
    }

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
    ///     走「实例化背景场景」而非原版 <c>NCombatBackground.Create</c>（需自定义场景路径）。
    ///     场景根必须挂 <see cref="FgoVoidSeaBackgroundRoot" />（继承 <c>NCombatBackground</c>），否则
    ///     <c>Instantiate&lt;NCombatBackground&gt;</c> 抛 <c>InvalidCastException</c>；
    ///     自带的 <c>Layer_00</c> 容器名也必须与查找键一致，否则拿不到容器、图层挂不上。
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
    ///     等价原版 <c>NCombatBackground.AddLayer</c>（查同名子节点 → 实例化 → 挂上），但容器名由约定传入、不抛异常。
    ///     容器缺失时原版抛 <c>InvalidOperationException</c>；这里返回 false 交调用方放弃——
    ///     因为"挂了个无图层的背景"只会静默显示成空白，比直接失败更难查。
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

    /// <remarks>
    ///     只杀不建：<c>CreateTween()</c> 会立刻开跑，没加 Tweener 引擎会报 "started with no Tweeners"，
    ///     故纯取消旧补间的路径（如 <see cref="DiscardVoidSea" />）只能走这里。
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