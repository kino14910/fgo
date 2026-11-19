using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

/// <summary>
///     十王满腹·葛笼宴海（Jyuuou Manpuku Tsuzura no Enkai）: 多人专用宝具。
///     群伤之后抬高全队的满腹槽上限，附带额外最大生命值与宝具值，并净化全队的中毒与诅呪。
/// </summary>
public class JyuuouManpukuTsuzuraNoEnkai() : NobleCardModel(1, CardType.Attack, TargetType.AllEnemies)
{
    public const int FullnessCapacityPerPlay = 4;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FullnessPower>(),
        HoverTipFactory.FromPower<MaxHpPower>(),
        FgoHoverTipFactory.FromNp()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(27),
        ModCardVars.Power<MaxHpPower>(3),
        ModCardVars.Int("Np", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(2);
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combat)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var allies = combat.GetTeammatesOf(Owner.Creature).ToList();

        foreach (var ally in allies)
        {
            // 先保证每人有能力（首次施加带基础上限）再叠 +4；只 Apply 一次会让无能力者上限只有 4。
            await FullnessPower.Ensure(choiceContext, ally, Owner.Creature, this);
            await PowerCmd.Apply<FullnessPower>(choiceContext, ally, FullnessCapacityPerPlay,
                Owner.Creature, this);
        }

        await PowerCmd.Apply<MaxHpPower>(choiceContext, allies,
            DynamicVars[nameof(MaxHpPower)].BaseValue, Owner.Creature, this);

        var np = DynamicVars["Np"].BaseValue;
        foreach (var ally in allies)
            if (ally.Player is { Character: FgoCharacter } player)
                await FgoResCmd.ModifyNp(np, player);

        foreach (var ally in allies)
        foreach (var ailment in ally.Powers.Where(power => power is PoisonPower or CursePower).ToList())
            await PowerCmd.Remove(ailment);
    }
}