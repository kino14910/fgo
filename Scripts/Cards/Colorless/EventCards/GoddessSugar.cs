using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.EventCards;

/// <summary>
///     配合遗物[gold]女神的砂糖[/gold]使用: 打出时获得好感度。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class GoddessSugar() : FgoBaseCardModel(1, CardType.Attack,
    CardRarity.Token, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(6),
        ModCardVars.Power<CriticalDamagePower>(10),
        ModCardVars.Int("Np", 10),
        ModCardVars.Power<PoisonPower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
        DynamicVars["CriticalDamagePower"].UpgradeValueBy(10);
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["CriticalDamagePower"].BaseValue, Owner.Creature, this);

        await PowerCmd.Apply<PoisonPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(PoisonPower)].BaseValue, Owner.Creature, this);

        await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, Owner);
    }
}