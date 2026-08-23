using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards;

/// <summary>
///     幼儿退化：获得能量，去除所有额外最大生命值（扣除对应的最大生命上限），
///     每去除 9 点获得 Np% 宝具值。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class InfantileRegression() : FgoBaseCardModel(0, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Energy(1),
        ModCardVars.Int("Np", 10)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(10); // 10 -> 20
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        // 读取全部额外最大生命值（MaxHpPower 的堆叠即 +1 额外生命/点）。
        var maxHpPower = Owner.Creature.GetPower<MaxHpPower>();
        if (maxHpPower == null) return;

        var extraHp = maxHpPower.Amount;

        // 去除所有额外最大生命值：移除后 MaxHpPower.AfterRemoved 会把最大生命上限扣回。
        await PowerCmd.Remove<MaxHpPower>(Owner.Creature);

        // 每去除 9 点，获得 Np% 宝具值。
        var pairs = extraHp / 9;
        if (pairs <= 0) return;
        await FgoResCmd.ModifyNp((int)(pairs * DynamicVars["Np"].BaseValue), Owner);
    }
}