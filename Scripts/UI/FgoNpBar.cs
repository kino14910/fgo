using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Fgo.Scripts.UI;

/// <summary>
///     控制器节点：附加到玩家角色（NCreature）上，驱动 guda.tscn 中
///     实例化的 NpBar 子树，根据 <see cref="FgoPlayerResources.Np" /> 更新宝具值进度条。
///     通过订阅 <see cref="FgoPlayerResources.NpChanged" /> 事件实现实时更新；
///     _Process 仅用于可见性管理、延迟订阅以及作为兜底轮询。
///     NpButton 点击触发 <see cref="FgoNoblePhantasmCmd.TryChooseNoblePhantasm" /> 弹出网格选卡。
/// </summary>
public sealed partial class FgoNpBar : Node
{
    private const int NpMax = 300;

    private TextureProgressBar? _bar;
    private Label? _label;
    private Control? _npBarRoot;
    private TextureButton? _button;
    private Player? _player;
    private FgoPlayerResources? _subscribed;
    private int _lastNp = -1;

    public static void Initialize()
    {
        ModNodeAttachmentRegistry
            .For(Entry.ModId)
            .RegisterReadyChild<NCreature, FgoNpBar>(
                "np_bar",
                static _ => new FgoNpBar(),
                new NodeAttachmentOptions
                {
                    Name = "FgoNpBar",
                    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName,
                });
    }

    public override void _Ready()
    {
        // 仅对玩家角色生效；敌人 creature 上的控制器直接停用。
        if (GetParent() is not NCreature creature || creature.Entity?.IsPlayer != true)
        {
            SetProcess(false);
            return;
        }

        _npBarRoot = creature.Visuals.GetNodeOrNull<Control>("NpBar");
        if (_npBarRoot == null)
        {
            SetProcess(false);
            return;
        }

        _player = creature.Entity?.Player;

        _bar = _npBarRoot.GetNodeOrNull<TextureProgressBar>("Bar")
               ?? _npBarRoot.FindChild("Bar", true, false) as TextureProgressBar;
        _label = _npBarRoot.GetNodeOrNull<Label>("NpLabel")
                 ?? _npBarRoot.FindChild("NpLabel", true, false) as Label;
        _button = _npBarRoot.GetNodeOrNull<TextureButton>("NpButton")
                  ?? _npBarRoot.FindChild("NpButton", true, false) as TextureButton;

        if (_button != null)
            _button.Pressed += OnNpButtonPressed;

        _npBarRoot.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_npBarRoot == null)
            return;

        bool inCombat =
            CombatManager.Instance != null &&
            (CombatManager.Instance.IsInProgress || CombatManager.Instance.IsStarting);

        if (!inCombat)
        {
            if (_npBarRoot.Visible)
                _npBarRoot.Visible = false;
            return;
        }

        // 卡牌选择（宝具网格、令咒选项等）弹出时隐藏 NpBar，
        // 避免其渲染在卡牌 grid 上方造成布局/视觉干扰。
        if (NOverlayStack.Instance != null && NOverlayStack.Instance.ScreenCount > 0)
        {
            if (_npBarRoot.Visible)
                _npBarRoot.Visible = false;
            return;
        }

        var resources = ModelDb.Singleton<FgoPlayerResources>();
        if (resources == null)
        {
            if (_npBarRoot.Visible)
                _npBarRoot.Visible = false;
            return;
        }

        // 延迟订阅：combat singleton 创建后立即挂上事件监听
        if (_subscribed != resources)
            TrySubscribe(resources);

        // 兜底轮询：捕获任何可能遗漏的事件
        var np = resources.Np;
        if (np != _lastNp)
            OnNpChanged(np);

        if (!_npBarRoot.Visible)
            _npBarRoot.Visible = true;
    }

    public override void _ExitTree()
    {
        if (_button != null)
            _button.Pressed -= OnNpButtonPressed;

        if (_subscribed != null)
        {
            _subscribed.NpChanged -= OnNpChanged;
            _subscribed = null;
        }
    }

    private void TrySubscribe(FgoPlayerResources resources)
    {
        if (_subscribed != null)
            _subscribed.NpChanged -= OnNpChanged;

        _subscribed = resources;
        _subscribed.NpChanged += OnNpChanged;
        // 订阅时立即同步一次，避免首帧空白
        OnNpChanged(resources.Np);
    }

    private void OnNpChanged(int np)
    {
        _lastNp = np;

        if (_bar != null)
        {
            _bar.Value = np;
            _bar.Modulate = np switch
            {
                >= 300 => new Color(1f, 0.84f, 0f),     // 金色：满值 / Overcharge 2
                >= 200 => new Color(1f, 0.6f, 0.2f),    // 橙色：Overcharge 1
                >= 100 => new Color(1f, 0.85f, 0.3f),   // 黄色：可释放宝具
                _ => new Color(0.35f, 0.5f, 1f),        // 蓝色：蓄能中
            };
        }

        if (_label != null)
            _label.Text = $"{np}";

        // 按钮可见性：NP >= 100 才可见且可点击（不可见自然不接收输入）
        if (_button != null)
        {
            bool canUse = np >= 100;
            _button.Visible = canUse;
            _button.Disabled = !canUse;
        }
    }

    private void OnNpButtonPressed()
    {
        if (_player == null) return;
        if (CombatManager.Instance == null) return;
        // 推迟一帧确保 Godot 布局已完成，避免 NCardGrid 在 Size=0 时初始化
        CallDeferred(nameof(DoNpButtonPressed));
    }

    private async void DoNpButtonPressed()
    {
        if (_player == null) return;
        if (CombatManager.Instance == null) return;

        var choiceContext = new BlockingPlayerChoiceContext();
        await FgoNoblePhantasmCmd.TryChooseNoblePhantasm(choiceContext, _player);
    }
}
