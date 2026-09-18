using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Fields;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
// ReSharper disable once RedundantUsingDirective
using Fgo.Scripts;

namespace Fgo.Scripts.UI;

public sealed partial class FgoGlobalHud : Control
{
    /// <summary>场地行里每个场地图标的显示边长（像素）。</summary>
    private const float FieldIconSize = 64f;

    /// <summary>场地行距屏幕顶部边缘的偏移（像素）。</summary>
    private const float FieldRowTop = 120f;

    /// <summary>场地行所在横带的高度（像素）。CenterContainer 需要一条有高度的横带才能水平居中。</summary>
    private const float FieldRowHeight = 88f;

    /// <summary>层数角标预留的方框边长（像素）；实际显示尺寸由字号决定，这个框只用于定位。</summary>
    private const float FieldBadgeSize = 30f;

    /// <summary>层数角标自图标右下角向内收缩的距离（像素），避免贴着边。</summary>
    private const float FieldBadgeInset = 2f;

    /// <summary>层数角标的字号（比场地名称小一号，压在图标右下角）。</summary>
    private const int FieldBadgeFontSize = 22;

    private static readonly Color DisabledModulate = new(1, 1, 1, 0.35f);

    /// <summary>
    ///     活动实例注册表。原先 FindAll() 每帧从树根递归遍历整个场景树
    ///     （战斗中数千节点，每帧两次），改为创建/销毁时注册、注销，
    ///     消除逐帧全树遍历带来的帧率开销。
    /// </summary>
    private static readonly List<FgoGlobalHud> Instances = [];

    /// <summary>
    ///     令咒贴图(0-3)在 Initialize 时一次性预加载。
    ///     原先每次数值变化才 GD.Load（更早版本每帧 GD.Load），
    ///     磁盘/资源缓存查找开销会拖累帧率；现在 Refresh 只换引用。
    /// </summary>
    private static readonly Texture2D?[] CommandSpellTextures = new Texture2D[4];

    private static bool _lastVisible;

    /// <summary>
    ///     是否已观察到战斗激活。开局 CombatManager.IsStarting 尚未置位时
    ///     不能直接关 _Process，否则会永久错过战斗开始。
    /// </summary>
    private bool _combatSeenActivated;

    private TextureButton _commandSpellButton = null!;

    // 场地行: 显示当前战场上的场地。按版本号做脏检查，仅在场地集合变化时重建。
    private CenterContainer _fieldRoot = null!;
    private HBoxContainer _fieldRow = null!;
    private bool _lastCanUse;
    private int _lastCommandSpell = -1;
    private int _lastFieldVersion = -1;

    // 脏检查缓存: 仅值变化时才触碰控件，避免每帧无条件 GD.Load/赋值导致的重绘。
    private int _lastStars = -1;

    // 〔虚数空间〕期间需要隐藏原版手牌区、并把额外手牌区对齐到原版位置，这里记上次的开关状态做脏检查。
    private bool _lastVoidSpace;
    private HBoxContainer _starBox = null!;
    private Label _starLabel = null!;

    // 缓存的额外手牌区容器（RitsuLib 挂在 NCombatUi 下），失效时重新查找。
    private NModExtraHand? _voidHandView;

    private static HoverTip CommandSpellHoverTip =>
        new(
            new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_COMMAND_SPELL.title"),
            new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_COMMAND_SPELL.description"));

    private static HoverTip StarHoverTip =>
        new(
            new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_STAR.title"),
            new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_STAR.description"));

    public static void Initialize()
    {
        for (var i = 0; i < CommandSpellTextures.Length; i++)
            CommandSpellTextures[i] = GD.Load<Texture2D>(
                $"res://Fgo/images/ui/CommandSpell/CommandSpell{i}.png");

        ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NCombatUi, FgoGlobalHud>(
                "global_hud",
                static _ => new FgoGlobalHud(),
                static (_, hud) => hud.Bind(),
                new NodeAttachmentOptions
                {
                    Name = "FgoGlobalHud",
                    DuplicatePolicy =
                        NodeAttachmentDuplicatePolicy.ReuseExistingByName
                });
    }

    public void Bind()
    {
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var vbox = new VBoxContainer();
        vbox.Name = "Root";
        vbox.SetAnchorsPreset(LayoutPreset.TopRight);
        vbox.OffsetLeft = -220;
        vbox.OffsetRight = 0;
        vbox.OffsetTop = 160;
        vbox.OffsetBottom = 560;
        vbox.Alignment = BoxContainer.AlignmentMode.Begin;
        vbox.AddThemeConstantOverride("separation", 24);

        AddChild(vbox);

        //---------------------------------------
        // Command Spell
        //---------------------------------------

        _commandSpellButton = new TextureButton();

        _commandSpellButton.CustomMinimumSize =
            new Vector2(128, 128);

        _commandSpellButton.StretchMode =
            TextureButton.StretchModeEnum.KeepAspectCentered;

        _commandSpellButton.Pressed += OnCommandSpellButtonPressed;
        _commandSpellButton.MouseEntered += OnCommandSpellMouseEntered;
        _commandSpellButton.MouseExited += OnCommandSpellMouseExited;

        vbox.AddChild(_commandSpellButton);

        //---------------------------------------
        // Fields
        //---------------------------------------

        // 场地是战场级属性（对所有单位生效），因此只在 HUD 上显示一次，不给每个生物各挂图标。
        // 位置: 顶部居中（TopCenter）。外层 CenterContainer 只占顶部一条横带（TopWide + 固定高度），
        // 由它把场地行水平居中；上下位置改 FieldRowTop，横带高度改 FieldRowHeight。
        _fieldRoot = new CenterContainer();
        _fieldRoot.Name = "FieldRoot";
        _fieldRoot.MouseFilter = MouseFilterEnum.Ignore;
        _fieldRoot.SetAnchorsPreset(LayoutPreset.TopWide);
        _fieldRoot.OffsetLeft = 0;
        _fieldRoot.OffsetRight = 0;
        _fieldRoot.OffsetTop = FieldRowTop;
        _fieldRoot.OffsetBottom = FieldRowTop + FieldRowHeight;
        AddChild(_fieldRoot);

        _fieldRow = new HBoxContainer();
        _fieldRow.Name = "FieldRow";
        _fieldRow.AddThemeConstantOverride("separation", 16);
        _fieldRow.Alignment = BoxContainer.AlignmentMode.Center;
        _fieldRow.MouseFilter = MouseFilterEnum.Ignore;
        _fieldRow.Visible = false;
        _fieldRoot.AddChild(_fieldRow);

        //---------------------------------------
        // Star
        //---------------------------------------

        var starRoot = new VBoxContainer();
        starRoot.Name = "StarRoot";
        starRoot.SetAnchorsPreset(LayoutPreset.TopLeft);
        starRoot.OffsetLeft = 80;
        starRoot.OffsetRight = 160;
        starRoot.OffsetTop = 690;
        starRoot.OffsetBottom = 700;
        starRoot.Alignment = BoxContainer.AlignmentMode.Center;
        AddChild(starRoot);

        var hbox = new HBoxContainer();

        hbox.AddThemeConstantOverride("separation", 8);
        // 整个 star 行作为 hover tip 的载体
        hbox.MouseFilter = MouseFilterEnum.Stop;
        hbox.MouseEntered += OnStarMouseEntered;
        hbox.MouseExited += OnStarMouseExited;
        _starBox = hbox;

        starRoot.AddChild(hbox);

        var starIcon = new Label();

        starIcon.Text = "✨";
        // 让父 HBoxContainer 接收鼠标事件
        starIcon.MouseFilter = MouseFilterEnum.Pass;

        starIcon.AddThemeFontSizeOverride(
            "font_size",
            48);

        hbox.AddChild(starIcon);

        _starLabel = new Label();

        _starLabel.Text = "0";
        // 让父 HBoxContainer 接收鼠标事件
        _starLabel.MouseFilter = MouseFilterEnum.Pass;

        _starLabel.AddThemeFontSizeOverride(
            "font_size",
            42);

        hbox.AddChild(_starLabel);

        Visible = false;

        Instances.Add(this);
    }

    public override void _Process(double delta)
    {
        var inCombat = FgoCombatUi.Update();

        if (inCombat)
        {
            _combatSeenActivated = true;
            return;
        }

        // 战斗尚未激活（开局 IsStarting 未置位）或仅处于暂停时保持轮询，
        // 否则会错过恢复时机导致 HUD 永久隐藏。
        if (!_combatSeenActivated || CombatManager.Instance.IsPaused)
            return;

        // 战斗已结束: 本节点即将随战斗场景销毁，关掉轮询避免空跑。
        // 下局战斗会重建实例（_Process 默认开启），BeforeCombatStart 兜底唤醒。
        SetProcess(false);
    }

    public static void SetHudVisible(bool visible)
    {
        if (visible == _lastVisible) return;
        _lastVisible = visible;
        foreach (var hud in Instances) hud.Visible = visible;
    }

    public static void Update()
    {
        foreach (var hud in Instances) hud.Refresh();
    }

    /// <summary>
    ///     新战斗开始时的兜底唤醒: 上一场战斗结束关掉的 _Process 重新开启，
    ///     并重置「已进入战斗」标记，防止复用实例时把开局误判为战斗结束。
    /// </summary>
    public static void WakeInstances()
    {
        foreach (var hud in Instances)
        {
            hud._combatSeenActivated = false;
            // 新战斗的场地集合从空开始，版本号会归 0，必须让脏检查失效以重建场地行。
            hud._lastFieldVersion = -1;
            // 上一场战斗若停在虚数空间，手牌区可能还是隐藏的，这里强制下次刷新时恢复。
            hud._lastVoidSpace = false;
            hud._voidHandView = null;
            hud.SetProcess(true);
        }
    }

    private void Refresh()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state);

        // 仅在本地玩家是 FGO 角色时显示令咒 / 暴击星。
        // 多人模式下不能回退到 Players.FirstOrDefault()，那会拿到错误的（主机）玩家。
        if (player is null || player.Character is not FgoCharacter)
        {
            if (Visible) Visible = false;
            // 本地玩家不是 FGO 角色（角色不会在战斗中改变）: 本场战斗内
            // HUD 持续隐藏，无需继续轮询；下局战斗由 BeforeCombatStart 兜底唤醒。
            SetProcess(false);
            return;
        }

        if (!Visible) Visible = true;

        var resources = FgoBattleHooks.Get(player);

        // 脏检查: 每帧只读数值，仅变化时更新控件，避免无条件 GD.Load / Text 赋值。
        if (resources.Stars != _lastStars)
        {
            _lastStars = resources.Stars;
            _starLabel.Text = _lastStars.ToString();
        }

        var commandSpell = Math.Clamp(resources.CommandSpell, 0, 3);
        if (commandSpell != _lastCommandSpell)
        {
            _lastCommandSpell = commandSpell;
            _commandSpellButton.TextureNormal = CommandSpellTextures[commandSpell];
        }

        var canUse = resources.CanUseCommandSpell;
        if (canUse != _lastCanUse)
        {
            _lastCanUse = canUse;
            _commandSpellButton.Modulate = canUse ? Colors.White : DisabledModulate;
            _commandSpellButton.Disabled = !canUse;
        }

        var fieldVersion = FgoField.VersionOf(state);
        if (fieldVersion != _lastFieldVersion)
        {
            _lastFieldVersion = fieldVersion;
            RebuildFieldRow(state);
        }

        SyncVoidSpaceHand(state);
    }

    /// <summary>
    ///     〔虚数空间〕期间隐藏原版手牌区的牌、并把额外手牌区搬到原版手牌区的位置。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <b>只藏 <c>CardHolderContainer</c>，绝不能藏整个 <c>NPlayerHand</c>。</b>
    ///         原版出牌时 <c>NPlayerHand.StartCardPlay</c> 会把被拿起的 holder
    ///         <c>Reparent(this)</c>（挂到 NPlayerHand 自己身上，而不是牌位容器里），并把拖动/目标预览节点
    ///         <c>NMouseCardPlay</c> 也 <c>AddChild</c> 到 NPlayerHand 下。父节点一旦不可见，拖动跟手、
    ///         目标选择时的居中预览就全部消失——只剩"点一下能出牌"。而原版手牌的牌位恰好全部住在
    ///         <c>CardHolderContainer</c> 里（<c>AddCardHolder</c> 只往它里面塞 holder），所以藏它一个就够。
    ///     </para>
    ///     <para>
    ///         真实手牌已被 <c>FgoVoidHand</c> 搬进无界面的暂存牌堆，但期间**新抽到的牌仍会落进原版手牌**，
    ///         不隐藏的就会和额外手牌区（同一套手牌扇形布局）叠在一起。这里只改本机 UI，属表现层同步。
    ///     </para>
    /// </remarks>
    private void SyncVoidSpaceHand(ICombatState? combat)
    {
        var inVoidSpace = FgoField.Has(combat, FgoFieldId.ImaginarySpace);
        if (inVoidSpace != _lastVoidSpace)
        {
            _lastVoidSpace = inVoidSpace;
            if (NPlayerHand.Instance?.CardHolderContainer is { } handCards)
                handCards.Visible = !inVoidSpace;
            _voidHandView = null;
        }

        if (!inVoidSpace) return;

        if (_voidHandView is null || !IsInstanceValid(_voidHandView))
            _voidHandView = FindVoidHandView();

        if (_voidHandView is not { } view || NPlayerHand.Instance is not { } vanilla) return;
        if (vanilla.CardHolderContainer is not { } holder) return;

        // 手牌扇形坐标的原点: 原版是 CardHolderContainer（NPlayerHand.tscn 里锚点 (0.5, 1) 的零尺寸
        // Control，即屏幕底边中点），RitsuLib 则是「容器左上角 + Size * 0.5」（见 NModExtraHand.ArrangeCards
        // 的 center）。两者对齐后两边的 HandPosHelper 偏移就落在同一点上。
        // 取「相对手牌节点」的偏移而不是全局坐标，是为了剔除手牌节点自身在入场(0→500)与禁用(+100)
        // 动画里的平移——额外手牌区有自己那套等价的禁用表现（DisabledOffset 默认也是 (0,100)），跟着动会叠两倍。
        var origin = holder.GlobalPosition - vanilla.GlobalPosition;
        var aligned = origin - view.Size * 0.5f;
        if (view.Position != aligned) view.Position = aligned;
    }

    /// <summary>找出〔虚数空间〕额外手牌区的容器节点（由 RitsuLib 挂在 NCombatUi 下）。</summary>
    private static NModExtraHand? FindVoidHandView()
    {
        return NCombatRoom.Instance?.Ui?.GetChildren()
            .OfType<NModExtraHand>()
            .FirstOrDefault(static view => view.Definition.PileType.Equals(FgoEnums.VoidHand));
    }

    /// <summary>
    ///     按当前场地集合重建场地行的子节点: 每个场地一个「图标 + 层数」的可悬停条目。
    ///     只在版本号变化时调用，避免逐帧增删节点。
    /// </summary>
    private void RebuildFieldRow(ICombatState? combat)
    {
        foreach (var child in _fieldRow.GetChildren())
        {
            if (child is Control control) NHoverTipSet.Remove(control);
            _fieldRow.RemoveChild(child);
            child.QueueFree();
        }

        var active = FgoField.ActiveOf(combat).OrderBy(static id => (int)id).ToList();
        _fieldRow.Visible = active.Count > 0;

        foreach (var id in active)
        {
            var entry = new Control();
            entry.CustomMinimumSize = new Vector2(FieldIconSize, FieldIconSize);
            // 用 Pass 而不是 Stop: 悬停提示照常触发，但鼠标事件继续往下传，
            // 万一场地行压到可点区域也不会吞掉点击。
            entry.MouseFilter = MouseFilterEnum.Pass;

            if (id.Icon() is { } icon)
            {
                var texture = new TextureRect();
                texture.Texture = icon;
                texture.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                texture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                texture.MouseFilter = MouseFilterEnum.Ignore;
                entry.AddChild(texture);
            }
            else
            {
                // 图标缺失（资源未导入）时退化为文字，保证场地仍然可见。
                var fallback = new Label();
                fallback.Text = id.ToTitle();
                fallback.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                fallback.HorizontalAlignment = HorizontalAlignment.Center;
                fallback.VerticalAlignment = VerticalAlignment.Center;
                fallback.MouseFilter = MouseFilterEnum.Ignore;
                fallback.AddThemeFontSizeOverride("font_size", 28);
                entry.AddChild(fallback);
            }

            var stacks = FgoField.StacksOf(combat, id);
            if (stacks > 1)
            {
                var count = new Label();
                count.Text = stacks.ToString();
                count.MouseFilter = MouseFilterEnum.Ignore;
                count.AddThemeFontSizeOverride("font_size", FieldBadgeFontSize);
                // 数字直接压在图标上容易糊，加一圈描边保证可读。
                count.AddThemeConstantOverride("outline_size", 4);
                count.AddThemeColorOverride("font_outline_color", Colors.Black);
                // 锚到右下角并向内收缩; GrowDirection.Begin 让文字超出预留框时朝左上生长，
                // 这样无论几位数都贴着右下角，不会溢出到图标外。
                count.SetAnchorsPreset(LayoutPreset.BottomRight);
                count.GrowHorizontal = GrowDirection.Begin;
                count.GrowVertical = GrowDirection.Begin;
                count.OffsetLeft = -FieldBadgeSize;
                count.OffsetTop = -FieldBadgeSize;
                count.OffsetRight = -FieldBadgeInset;
                count.OffsetBottom = -FieldBadgeInset;
                count.HorizontalAlignment = HorizontalAlignment.Right;
                count.VerticalAlignment = VerticalAlignment.Bottom;
                entry.AddChild(count);
            }

            var fieldId = id;
            entry.MouseEntered += () =>
                NHoverTipSet.CreateAndShow(entry, fieldId.ToHoverTip(), HoverTipAlignment.Left);
            entry.MouseExited += () => NHoverTipSet.Remove(entry);

            _fieldRow.AddChild(entry);
        }
    }

    public override void _ExitTree()
    {
        Instances.Remove(this);

        _commandSpellButton.Pressed -= OnCommandSpellButtonPressed;
        _commandSpellButton.MouseEntered -= OnCommandSpellMouseEntered;
        _commandSpellButton.MouseExited -= OnCommandSpellMouseExited;
        NHoverTipSet.Remove(_commandSpellButton);

        _starBox.MouseEntered -= OnStarMouseEntered;
        _starBox.MouseExited -= OnStarMouseExited;
        NHoverTipSet.Remove(_starBox);

        if (_fieldRow != null)
            foreach (var child in _fieldRow.GetChildren())
                if (child is Control control)
                    NHoverTipSet.Remove(control);
    }

    private static void OnCommandSpellButtonPressed()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state);
        if (player is null || player.Character is not FgoCharacter) return;

        // 选牌作为托管网络动作走官方动作队列（药水 UsePotionAction 模式），
        // 在所有 peer 上执行；此前直接在 UI 事件里跑选牌会让 host 侧
        // 永远等不到 SetChoiceContext，动作队列死锁、全游戏卡死。
        FgoCommandSpellCmd.Request();
    }

    private void OnCommandSpellMouseEntered()
    {
        NHoverTipSet.CreateAndShow(_commandSpellButton, CommandSpellHoverTip, HoverTipAlignment.Left);
    }

    private void OnCommandSpellMouseExited()
    {
        NHoverTipSet.Remove(_commandSpellButton);
    }

    private void OnStarMouseEntered()
    {
        NHoverTipSet.CreateAndShow(_starBox, StarHoverTip, HoverTipAlignment.Center);
    }

    private void OnStarMouseExited()
    {
        NHoverTipSet.Remove(_starBox);
    }
}