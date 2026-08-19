using Fgo.Scripts.Cards.Colorless;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class SwifterThanSound() : FgoCardModel(1, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies)
{
    private const int DurationTurns = 2;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<InfiniteSuffering>(),
        HoverTipFactory.FromCard<TheAbsoluteSword>(),
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Damage(8)];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<SwifterThanSoundPower>(choiceContext, 
            Owner.Creature, DurationTurns, Owner.Creature, this);
        await PowerCmd.Apply<SwifterThanSoundCardPower>(choiceContext, 
            Owner.Creature, DurationTurns, Owner.Creature, this);
    }
}