using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class PeerlessStrike() : FgoCardModel(0, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalDamagePower>(DynamicVars[nameof(CriticalDamagePower)].IntValue)
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(48),
        ModCardVars.Power<CriticalDamagePower>(100)
    ];

    protected override bool IsPlayable =>
        Owner.Creature.CurrentHp <= Owner.Creature.MaxHp / 2;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(12);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(CriticalDamagePower)].BaseValue,
            Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);
        // 升级前立即死亡，升级后回合结束时死亡
        if (IsUpgraded)
            await PowerCmd.Apply<PeerlessStrikePower>(choiceContext, Owner.Creature, 9999m, Owner.Creature, this);
        else
            await CreatureCmd.Kill(Owner.Creature);
    }
}