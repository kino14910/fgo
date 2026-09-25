using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;

namespace Fgo.Scripts.Powers;

/// <summary>
///     饱腹: 〔回转膳食〕体系的「满腹槽上限 + 当前饱腹」，合并成一个能力。
///     <list type="bullet">
///         <item><c>Amount</c> = 满腹槽上限（含基础值），画在图标**右上**；</item>
///         <item><c>DynamicVars["Fullness"]</c> = 当前饱腹，画在图标**右下**；</item>
///         <item>每张菜品卡 +2 层，达到上限后菜品卡完全空转（见 <see cref="CanEat" />）；</item>
///         <item>每打出一张「有费用的攻击卡」-1 层。</item>
///     </list>
/// </summary>
/// <remarks>
///     两处与 <see cref="TerrorPower" /> 同源的做法:
///     <para>
///         ① 角标走 <see cref="IPowerExtraIconAmountLabelSpecsProvider" />，并且 <see cref="StackType" /> 取
///         <c>None</c> —— 原版数量标签只对 <c>Counter</c> 出字（<c>NPower.RefreshAmount</c> 里是
///         <c>StackType == Counter ? DisplayAmount : ""</c>），取 None 正好把原版标签腾空，避免和自己画的
///         两个数字重复。改完值要手动 <c>InvokeDisplayAmountChanged()</c> 才会刷新角标。
///     </para>
///     <para>
///         ② 上限放进 <c>Amount</c> 而不是另开一个 DynamicVar，是为了能用 <c>PowerCmd.Apply</c> 施加 ——
///         <c>Apply</c> 对 amount 0 直接早退，没法"创建一个 0 层的能力"；把基础上限(4)当作首次施加的 amount
///         就绕开了这个限制，而且 <c>Amount &gt; 0</c> 让能力不会被 <c>ShouldRemoveDueToAmount</c> 自动移除，
///         十王满腹 给没有吃过菜的盟友抬上限也只需要 Apply(+4)。
///     </para>
/// </remarks>
public class FullnessPower : FgoPowerModel, IPowerExtraIconAmountLabelSpecsProvider
{
    /// <summary>基础上限。首次施加时就是这个值。</summary>
    public const int BaseCapacity = 4;

    /// <summary>每张菜品卡累积的层数。</summary>
    public const int PerDish = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Fullness", 0)
    ];

    /// <summary>当前饱腹（右下角标）。</summary>
    public int Fullness => DynamicVars["Fullness"].IntValue;

    /// <summary>满腹槽上限（右上角标）。</summary>
    public int Capacity => Amount;

    /// <summary>目标当前的上限；没有饱腹能力时回落到基础上限。</summary>
    public static int CapacityOf(Creature creature)
    {
        return creature.GetPower<FullnessPower>()?.Capacity ?? BaseCapacity;
    }

    /// <summary>
    ///     是否还能继续「吃」。为 false 时菜品卡完全空转: 不加饱腹、不给暴击星、菜品自身效果也失效。
    /// </summary>
    public static bool CanEat(Player player)
    {
        return player.Creature.GetPower<FullnessPower>() is not { } power || power.Fullness < power.Capacity;
    }

    /// <summary>
    ///     确保目标身上有〔饱腹〕且上限已含基础值；已经有就原样返回，**不会重复叠加基础值**。
    ///     首次施加走 <see cref="BaseCapacity" /> 这个非零 amount。
    /// </summary>
    public static async Task<FullnessPower?> Ensure(PlayerChoiceContext choiceContext, Creature creature,
        Creature? applier, CardModel? cardSource)
    {
        if (creature.GetPower<FullnessPower>() is { } existing) return existing;

        return await PowerCmd.Apply<FullnessPower>(choiceContext, creature, BaseCapacity, applier, cardSource);
    }

    /// <summary>累积饱腹，钳到上限。</summary>
    public void Gain(int amount)
    {
        SetFullness(Fullness + amount);
    }

    /// <summary>消耗饱腹（消化/重置用），不会低于 0。</summary>
    public void Lose(int amount)
    {
        SetFullness(Fullness - amount);
    }

    private void SetFullness(int value)
    {
        var clamped = Math.Clamp(value, 0, Capacity);
        if (clamped == Fullness) return;

        DynamicVars["Fullness"].BaseValue = clamped;
        // 角标由本类自己画，不经过 SetAmount 那条 DisplayAmountChanged 通路，必须手动通知刷新。
        InvokeDisplayAmountChanged();
    }

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            // 右上: 满腹槽上限
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.TopRight, Capacity.ToString()),
            // 右下: 当前饱腹
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.BottomRight, Fullness.ToString())
        ];
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner) return;
        if (cardPlay.Card is not { Type: CardType.Attack }) return;
        if (!CostsEnergy(cardPlay)) return;
        if (Fullness <= 0) return;

        Flash();
        Lose(1);
    }

    /// <summary>
    ///     「有费用的攻击卡」按**卡面费用**判定（不含临时增减费），X 费牌一律算有费用。
    ///     这样"这张牌本身要不要花能量"是稳定的，玩家不会因为一次减费效果就消化不了。
    /// </summary>
    private static bool CostsEnergy(CardPlay cardPlay)
    {
        return cardPlay.Card.EnergyCost.Canonical > 0 || cardPlay.Card.EnergyCost.CostsX;
    }
}
