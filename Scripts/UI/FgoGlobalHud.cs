using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
// ReSharper disable once RedundantUsingDirective
using Fgo.Scripts;

namespace Fgo.Scripts.UI;

public sealed partial class FgoGlobalHud : Control
{
    /// <summary>
    ///     HoverTip 构造时会立即解析本地化文本，而 mod 本地化表在 Init 之后才加载。
    ///     FgoGlobalHud 的静态构造在 Entry.Init → FgoCombatUi.Initialize 时触发，
    ///     若在 cctor 里创建 HoverTip 会抛 TypeInitializationException 导致整个 mod 加载失败。
    ///     因此延迟到首次鼠标悬停（战斗中）时才构造，那时本地化已就绪。
    /// </summary>
    private static HoverTip? _commandSpellHoverTip;

    private static HoverTip? _starHoverTip;

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
    private bool _lastCanUse;
    private int _lastCommandSpell = -1;

    // 脏检查缓存: 仅值变化时才触碰控件，避免每帧无条件 GD.Load/赋值导致的重绘。
    private int _lastStars = -1;
    private HBoxContainer _starBox = null!;
    private Label _starLabel = null!;

    private static HoverTip CommandSpellHoverTip =>
        _commandSpellHoverTip ??= new HoverTip(
            new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_COMMAND_SPELL.title"),
            new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_COMMAND_SPELL.description"));

    private static HoverTip StarHoverTip =>
        _starHoverTip ??= new HoverTip(
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
        // Star
        //---------------------------------------

        var hbox = new HBoxContainer();

        hbox.AddThemeConstantOverride("separation", 8);
        // 整个 star 行作为 hover tip 的载体
        hbox.MouseFilter = MouseFilterEnum.Stop;
        hbox.MouseEntered += OnStarMouseEntered;
        hbox.MouseExited += OnStarMouseExited;
        _starBox = hbox;

        vbox.AddChild(hbox);

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
        NHoverTipSet.CreateAndShow(_starBox, StarHoverTip, HoverTipAlignment.Left);
    }

    private void OnStarMouseExited()
    {
        NHoverTipSet.Remove(_starBox);
    }
}