using Fgo.Scripts.Cards;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

/// <summary>
///     领袖气质（Charisma）所授予的能力。
///     <list type="bullet">
///         <item>持有期间，你可以打出能量不足的卡牌 —— 门槛由 <c>FgoCharismaCostPatch</c> 解除；</item>
///         <item>手牌中只能靠本能力超能量额打出的卡发紫光描边（<see cref="ShouldGlowPurple" />，
///         经 RitsuLib <c>ModCardHandOutlineRegistry</c> 渲染 —— 原版描边只有青/金/红三色）；</item>
///         <item>每缺少 1 点能量，失去 1 点生命并获得 1 层[gold]疲劳[/gold]；</item>
///         <item>生命损失不受升级影响。</item>
///     </list>
/// </summary>
/// <remarks>
///     缺口结算放在能力里（而不是某张卡里），是因为「能量不足可打出」本身就是这条能力的效果，
///     对<see cref="CardModel">任意卡牌</see>生效；能力被移除后门槛立刻恢复。<br />
///     疲劳是[gold]领袖气质[/gold]卡牌自身的持久化数值（TheScythe 模式），这里只负责把缺口转成疲劳写入该卡，
///     再由该卡在打出时做[gold]灾厄[/gold]判定。<br />
///     <b>缺口不要用 <c>CardPlay.Resources</c> 算</b>：<c>PlayCardAction</c> 会把
///     <c>EnergyValue</c> 与 <c>EnergySpent</c> 赋成同一个值，两者相减恒为 0。
/// </remarks>
public class CharismaPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    ///     [gold]超能量击[/gold]紫光描边色。亮度/透明度对齐原版 <c>NCardHighlight.gold</c>（金）、
    ///     <c>NCardHighlight.red</c>（红）的取值语法（alpha 0.98）。
    /// </summary>
    public static readonly Color GlowColor = new(0.68f, 0.42f, 0.98f, 0.98f);

    /// <summary>
    ///     判定卡牌是否应显示[gold]超能量击[/gold]紫光：拥有者持有本能力、当前处于出牌阶段、
    ///     卡牌其余条件均可打出（<see cref="CardModel.CanPlay" />），且应支付能量超过剩余能量
    ///     —— 即"只能靠本能力超能量额打出"的卡。
    ///     <para>
    ///         能量门槛的解除见 <c>FgoCharismaCostPatch</c>（清除 <c>UnplayableReason.EnergyCostTooHigh</c>），
    ///         所以这里的 <see cref="CardModel.CanPlay" /> 对超能卡必然为真；不可打出原因若是别的
    ///         （Unplayable 关键词、BlockedByHook、星费等），<see cref="CardModel.CanPlay" /> 为假，
    ///         连同原版 holder 门控一起挡下，不会误亮，也不会覆盖红/金等其他描边。
    ///     </para>
    ///     <para>
    ///         X 费牌的 <c>EnergyCost.GetAmountToSpend()</c> 恒等于当前能量，永远不会触发。
    ///     </para>
    /// </summary>
    public static bool ShouldGlowPurple(CardModel card)
    {
        if (card.Owner?.PlayerCombatState is not { } combatState) return false;
        if (combatState.Phase != PlayerTurnPhase.Play) return false;
        if (card.EnergyCost.GetAmountToSpend() <= combatState.Energy) return false;
        if (!card.CanPlay()) return false;
        return card.Owner.Creature.HasPower<CharismaPower>();
    }

    /// <summary>
    ///     结算能量缺口。
    ///     <para>
    ///         <see cref="AbstractModel.AfterEnergySpent" /> 在 <c>SpendResources</c> → <c>SpendEnergy</c> 内部触发，
    ///         此时 <paramref name="amount" /> 是**未截断的应支付值**（<c>EnergyCost.GetAmountToSpend()</c> 的结果），
    ///         X 费牌也已在同一方法里先写入 <c>CapturedXValue</c>，可直接复读。
    ///     </para>
    ///     <para>
    ///         应支付值 - 当前剩余能量 = 缺口。缺口为 0 时 <c>LoseEnergy</c> 不会被执行（<c>amount &gt; 0</c> 才扣），
    ///         所以这里的读数就是扣费后的余量，无需补偿。
    ///     </para>
    /// </summary>
    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (amount <= 0) return;

        var player = Owner.Player;
        if (player == null) return;
        if (card.Owner != player) return;

        var energyLeft = player.PlayerCombatState?.Energy ?? 0;
        var missing = amount - energyLeft;
        if (missing <= 0) return;

        Flash();

        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), Owner, missing,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);

        Charisma.AddFatigue(player, card, missing);
    }
}
