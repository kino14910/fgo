using Fgo.Scripts.Cards.Colorless.Dishes;
using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Powers;

/// <summary>
///     回转膳食（Kurakura's Meals）: 每回合轮流发放一张菜品卡，并结算菜品卡打出时的附带收益。
/// </summary>
/// <remarks>
///     轮换状态（已发放数 / 下一个位置）保存在能力实例的字段里，跨回合持续，战斗结束随能力消失。
///     发放与掷骰都发生在 <see cref="AfterPlayerTurnStart" /> —— 该钩子在复制的回合开始流程里
///     于各端各执行一次，条件（发放数、轮换位）全是同步状态，因此各端消耗同一随机序列；
///     主卡打出时的首张发放则发生在被复制的出牌动作内，同样安全。
/// </remarks>
public class KurakurasMealsPower : FgoPowerModel
{
    /// <summary>自第 N 张菜品卡起，轮到炒荞麦面时开始判定变异。</summary>
    public const int MutationStartCount = 7;

    /// <summary>变异概率（百分比）。</summary>
    public const int MutationChancePercent = 5;

    /// <summary>
    ///     菜品轮换顺序。变异判定按 <see cref="Yakisoba" /> 类型匹配、不依赖位置，
    ///     所以调整顺序时只需保持类型与工厂一致。
    /// </summary>
    private static readonly (Type Type, Func<ICombatState, Player, CardModel> Create)[] Rotation =
    [
        (typeof(Yakisoba), static (combat, player) => combat.CreateCard<Yakisoba>(player)),
        (typeof(Curry), static (combat, player) => combat.CreateCard<Curry>(player)),
        (typeof(Ramen), static (combat, player) => combat.CreateCard<Ramen>(player)),
        (typeof(Frankfurt), static (combat, player) => combat.CreateCard<Frankfurt>(player)),
        (typeof(Skewers), static (combat, player) => combat.CreateCard<Skewers>(player)),
        (typeof(Parfait), static (combat, player) => combat.CreateCard<Parfait>(player))
    ];

    private int _granted;
    private int _nextIndex;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Star", 20)
    ];

    /// <summary>发放的菜品卡是否使用升级形态（由主卡的升级状态决定）。</summary>
    public bool UpgradedDishes { get; private set; }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _granted = 0;
        _nextIndex = 0;
        UpgradedDishes = false;
    }

    /// <summary>
    ///     由〔回转膳食〕打出时写入: 菜品卡附带的暴击星数，以及发放的菜品卡是否升级。
    ///     重复打出只刷新这两项，不重置轮换进度。
    /// </summary>
    public void Configure(decimal stars, bool upgradedDishes)
    {
        DynamicVars["Star"].BaseValue = stars;
        UpgradedDishes = upgradedDishes;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        Flash();
        await GrantNextDish(choiceContext, this, player);
    }

    /// <summary>
    ///     发放轮换中的下一张菜品卡。首张（炒荞麦面）由〔回转膳食〕的 OnPlay 直接调用，
    ///     之后每回合开始时由 <see cref="AfterPlayerTurnStart" /> 推进。
    /// </summary>
    public static async Task GrantNextDish(PlayerChoiceContext choiceContext, KurakurasMealsPower power, Player player)
    {
        var combat = player.Creature.CombatState;
        if (combat == null) return;

        var index = power._nextIndex % Rotation.Length;
        power._nextIndex = (power._nextIndex + 1) % Rotation.Length;
        power._granted++;

        // 自第 7 张起，轮到炒荞麦面时有 5% 概率变成传说中的饭团。
        // 短路求值保证"掷骰与否"只取决于同步状态，各端消耗的随机序列长度一致。
        var mutates = power._granted >= MutationStartCount
                      && Rotation[index].Type == typeof(Yakisoba)
                      && player.RunState.Rng.Niche.NextFloat() * 100f < MutationChancePercent;

        var card = mutates
            ? combat.CreateCard<LegendaryOnigiri>(player)
            : Rotation[index].Create(combat, player);

        if (power.UpgradedDishes) CardCmd.Upgrade(card);

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }

    /// <summary>
    ///     菜品卡打出后的附带收益: <c>20(30)</c> 颗暴击星 + 2 层饱腹。
    ///     饱腹已达上限时两者都不给（主卡描述里的「满腹后不再获得饱腹与菜品卡增益」）。
    /// </summary>
    public static async Task OnDishPlayed(PlayerChoiceContext choiceContext, Player player, CardModel dish)
    {
        if (!FullnessPower.CanEat(player)) return;
        if (player.Creature.GetPower<KurakurasMealsPower>() is not { } power) return;

        await FgoResCmd.ModifyStars(power.DynamicVars["Star"].IntValue, player);

        // 满腹槽上限与当前饱腹合并挂在同一个能力上；首次施加用基础上限做 amount（Apply 对 0 会早退）。
        var fullness = await FullnessPower.Ensure(choiceContext, player.Creature, player.Creature, dish);
        fullness?.Gain(FullnessPower.PerDish);
    }
}
