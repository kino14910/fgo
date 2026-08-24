using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class CharismaOfTheJade() : FgoCardModel(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(7),
        ModCardVars.Int("Hits", 3)
    ];

    protected override bool ShouldGlowGoldInternal =>
        IsMutable && Owner is not null && FgoBattleHooks.Get(Owner).CanSpecialCrit;

    protected override void OnUpgrade()
    {
        DynamicVars["Hits"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(DynamicVars["Hits"].IntValue)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        // 暴击由 FgoBattleHooks 处理: Stars>=20 时消耗20星造成300%伤害，否则按基础暴击（Stars>=10消耗10星200%）
    }
}