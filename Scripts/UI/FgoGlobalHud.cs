using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Fields;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

public sealed partial class FgoGlobalHud : Control
{
    private const float FieldIconSize = 64f;

    private const float FieldRowTop = 120f;

    /// <summary>场地行所在横带的高度（像素）。CenterContainer 需要一条有高度的横带才能水平居中。</summary>
    private const float FieldRowHeight = 88f;

    private const float FieldBadgeSize = 30f;

    /// <summary>层数角标自图标右下角向内收缩的距离（像素），避免贴着边。</summary>
    private const float FieldBadgeInset = 2f;

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

    private CenterContainer _fieldRoot = null!;
    private HBoxContainer _fieldRow = null!;
    private bool _lastCanUse;
    private int _lastCommandSpell = -1;
    private int _lastFieldVersion = -1;

    private int _lastStars = -1;

    private bool _lastVoidSpace;
    private HBoxContainer _starBox = null!;
    private Label _starLabel = null!;

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
        //---------------------------------------

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
        hbox.MouseFilter = MouseFilterEnum.Stop;
        hbox.MouseEntered += OnStarMouseEntered;
        hbox.MouseExited += OnStarMouseExited;
        _starBox = hbox;

        starRoot.AddChild(hbox);

        var starIcon = new Label();

        starIcon.Text = "✨";
        starIcon.MouseFilter = MouseFilterEnum.Pass;

        starIcon.AddThemeFontSizeOverride(
            "font_size",
            48);

        hbox.AddChild(starIcon);

        _starLabel = new Label();

        _starLabel.Text = "0";
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

        // 战斗未激活或暂停时保持轮询，否则错过恢复时机致 HUD 永久隐藏。
        if (!_combatSeenActivated || CombatManager.Instance.IsPaused)
            return;

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
            // 新战斗场地版本号归 0，须让脏检查失效以重建场地行。
            hud._lastFieldVersion = -1;
            hud._lastVoidSpace = false;
            hud._voidHandView = null;
            hud.SetProcess(true);
        }
    }

    private void Refresh()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state);

        // 多人下不能回退到 Players.FirstOrDefault()，那会拿到错误的（主机）玩家。
        if (player is null || player.Character is not FgoCharacter)
        {
            if (Visible) Visible = false;
            SetProcess(false);
            return;
        }

        if (!Visible) Visible = true;

        var resources = FgoBattleHooks.Get(player);

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

        SyncVoidSpaceHand(player);
    }

    /// <summary>
    ///     〔虚数空间〕期间隐藏原版手牌区的牌、并把额外手牌区搬到原版手牌区的位置。
    /// </summary>
    /// <remarks>只藏 CardHolderContainer，绝不藏整个 NPlayerHand：后者的 Visible=false 会让拖动跟手与目标预览一起消失。判定按本地玩家。</remarks>
    private void SyncVoidSpaceHand(Player? player)
    {
        var inVoidSpace = FgoVoidHand.IsInside(player);
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

        var origin = holder.GlobalPosition - vanilla.GlobalPosition;
        // 额外手牌区锚点在中心，须减半尺寸把它中心对齐到原手牌区原点。
        var aligned = origin - view.Size * 0.5f;
        if (view.Position != aligned) view.Position = aligned;
    }

    /// <summary>找出〔虚数空间〕额外手牌区的容器节点（由 RitsuLib 挂在 NCombatUi 下）。</summary>
    private static NModExtraHand? FindVoidHandView()
    {
        return NCombatRoom.Instance?.Ui.GetChildren()
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
                count.AddThemeConstantOverride("outline_size", 4);
                count.AddThemeColorOverride("font_outline_color", Colors.Black);
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

        foreach (var child in _fieldRow.GetChildren())
            if (child is Control control)
                NHoverTipSet.Remove(control);
    }

    private static void OnCommandSpellButtonPressed()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state);
        if (player is null || player.Character is not FgoCharacter) return;

        // 选牌须走托管动作队列：直接在 UI 事件里跑会让 host 等不到 SetChoiceContext、全游戏死锁。
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
        NHoverTipSet.CreateAndShow(_starBox, StarHoverTip);
    }

    private void OnStarMouseExited()
    {
        NHoverTipSet.Remove(_starBox);
    }
}