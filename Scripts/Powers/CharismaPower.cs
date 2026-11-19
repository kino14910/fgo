using Fgo.Scripts.Cards;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

/// <summary>
///     领袖气质所授予的能力：持有期间可打出能量不足的卡（门槛由 <c>FgoCharismaCostPatch</c> 解除），
///     每缺 1 点能量失去 1 点生命并获得 1 层疲劳（生命损失不受升级影响）。
///     只有靠本能力超能量额打出的卡发紫光描边（<see cref="ShouldGlowPurple" />）。
/// </summary>
/// <remarks>缺口在能力里结算（对任意卡生效、移除后门槛即恢复）。不能用 CardPlay.Resources 算：PlayCardAction 把 EnergyValue 与 EnergySpent 赋成同值，相减恒为 0。</remarks>
public class CharismaPower : FgoPowerModel
{
    /// <summary>超能量击紫光描边色（alpha 0.98 对齐原版描边的取值语法）。</summary>
    public static readonly Color GlowColor = new(0.68f, 0.42f, 0.98f, 0.98f);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <remarks>
    ///     门槛已由 FgoCharismaCostPatch 解除，故 CanPlay() 对超能卡为真、对其他不可打出的卡（Unplayable/BlockedByHook 等）为假——据此区分，不误亮也不覆盖红/金描边。X
    ///     费牌花费恒等于当前能量，不会触发。
    /// </remarks>
    public static bool ShouldGlowPurple(CardModel card)
    {
        if (card.Owner.PlayerCombatState is not { } combatState) return false;
        if (combatState.Phase != PlayerTurnPhase.Play) return false;
        if (card.EnergyCost.GetAmountToSpend() <= combatState.Energy) return false;
        if (!card.CanPlay()) return false;
        return card.Owner.Creature.HasPower<CharismaPower>();
    }

    /// <remarks>
    ///     <paramref name="amount" /> 是未截断的应支付值（X 费已写 CapturedXValue）；缺口 = 应支付 - 当前剩余。缺口为 0 时 LoseEnergy
    ///     不执行，故此处读数即扣费后余量，无需补偿。
    /// </remarks>
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