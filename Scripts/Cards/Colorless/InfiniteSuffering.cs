using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless;

[RegisterCard(typeof(TokenCardPool))]
public class InfiniteSuffering() : FgoBaseCardModel(2, CardType.Attack,
    CardRarity.Token, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<TheAbsoluteSword>(),
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(16)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<InfiniteSufferingPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}