using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class PeerlessStrike() : FgoCardModel(2, CardType.Attack,
        CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(24, ValueProp.Move),
        new IntVar("CritDamage", 100m)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }

    protected override bool IsPlayable =>
        Owner.Creature.CurrentHp <= Owner.Creature.MaxHp / 2;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature, DynamicVars["CritDamage"].BaseValue,
            Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_heavy")
            .Execute(choiceContext);
        // 升级前立即死亡，升级后回合结束时死亡
        if (IsUpgraded)
            await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 100m, Owner.Creature, this);
        else
            await CreatureCmd.Kill(Owner.Creature);
    }
}