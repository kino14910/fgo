using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     最后的度假胜地: 多人专用。开局自带 5 层冷却，之后每次出牌推进冷却，
///     冷却归零时为一整个队伍补满宝具值、宝具威力与毅力。
///     宝具值是按玩家分实例存放的运行时状态，只能逐个友方结算，不能靠施加状态完成。
/// </summary>
public class LastResort() : FgoCooldownCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override int CooldownMax => 12;

    public override int CombatStartCooldown => 5;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<GutsPower>(),
        FgoHoverTipFactory.FromNp()
    ];

    protected override IEnumerable<DynamicVar> AdditionalCanonicalVars =>
    [
        // 卡面「战斗开始时冷却{StartCooldown}」读它；值必须来自 CombatStartCooldown，避免和实际初始冷却脱节。
        ModCardVars.Int("StartCooldown", CombatStartCooldown),
        ModCardVars.Int("Np", 30),
        ModCardVars.Power<NpDamagePower>(20),
        ModCardVars.Power<GutsPower>(6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["CooldownMax"].UpgradeValueBy(-2);
        DynamicVars["Np"].UpgradeValueBy(20);
        DynamicVars[nameof(NpDamagePower)].UpgradeValueBy(10);
        DynamicVars[nameof(GutsPower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var allies = CombatState!.GetTeammatesOf(Owner.Creature).ToList();

        await PowerCmd.Apply<NpDamagePower>(choiceContext, allies,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<GutsPower>(choiceContext, allies,
            DynamicVars[nameof(GutsPower)].BaseValue, Owner.Creature, this);

        var np = DynamicVars["Np"].BaseValue;
        foreach (var ally in allies)
            if (ally.Player is { Character: FgoCharacter } allyPlayer)
                await FgoResCmd.ModifyNp(np, allyPlayer);
    }
}
