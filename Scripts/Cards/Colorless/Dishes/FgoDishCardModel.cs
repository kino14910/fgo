using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>
///     菜品卡基类（〔回转膳食〕Kurakura's Meals 轮换发放的 0 费 Token）。
///     <para>
///         统一的打出流程: 先判断是否满腹（满腹后**完全空转**——不加饱腹、不给暴击星、
///         菜品自身效果也失效），再结算菜品自身效果，最后结算〔回转膳食〕的附带收益
///         （20(30) 颗暴击星 + 2 层饱腹）。
///     </para>
/// </summary>
[RegisterCard(typeof(TokenCardPool), Inherit = true)]
public abstract class FgoDishCardModel()
    : FgoBaseCardModel(0, CardType.Power, CardRarity.Token, TargetType.AllAllies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override CardAssetProfile AssetProfile => new(
        "res://Fgo/images/cards/KurakurasMeals.png",
        "res://Fgo/images/cards/beta/KurakurasMeals.png",
        Type switch
        {
            CardType.Attack => "res://Fgo/images/card_frames/card_frame_attack.png",
            CardType.Skill => "res://Fgo/images/card_frames/card_frame_skill.png",
            CardType.Power => "res://Fgo/images/card_frames/card_frame_power.png",
            _ => "res://Fgo/images/card_frames/card_frame_skill.png"
        }
        );

    /// <summary>菜品卡的作用范围: 玩家方全队（含自己）。</summary>
    protected IReadOnlyList<Creature> Allies => CombatState!.GetTeammatesOf(Owner.Creature);

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!FullnessPower.CanEat(Owner)) return;

        await ApplyDishEffect(choiceContext);
        await KurakurasMealsPower.OnDishPlayed(choiceContext, Owner, this);
    }

    /// <summary>菜品自身效果（对 <see cref="Allies" /> 结算）。</summary>
    protected abstract Task ApplyDishEffect(PlayerChoiceContext choiceContext);

    /// <summary>
    ///     宝具值是 <c>FgoPlayerState</c> 里按玩家分实例的运行时状态，不是 Power，
    ///     只能逐个盟友结算，无法整列表施加。
    /// </summary>
    protected async Task GrantNpToAllies(decimal amount)
    {
        foreach (var ally in Allies)
            if (ally.Player is { Character: FgoCharacter } player)
                await FgoResCmd.ModifyNp(amount, player);
    }
}
