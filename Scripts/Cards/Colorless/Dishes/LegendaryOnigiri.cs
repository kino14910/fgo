using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless.Dishes;

/// <summary>传说中的饭团 / Legendary Onigiri: 其余所有菜品卡的效果各来一份。由〔回转膳食〕第 7 张起的变异产出。</summary>
public class LegendaryOnigiri : FgoDishCardModel
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<MaxHpPower>(),
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<StrengthPower>(1),
        ModCardVars.Power<MaxHpPower>(3),
        ModCardVars.Int("Np", 10),
        ModCardVars.Power<NpDamagePower>(10),
        ModCardVars.Power<CriticalDamagePower>(30),
        ModCardVars.Heal(20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(3);
        DynamicVars["Np"].UpgradeValueBy(10);
        DynamicVars[nameof(NpDamagePower)].UpgradeValueBy(10);
        DynamicVars[nameof(CriticalDamagePower)].UpgradeValueBy(20);
        DynamicVars.Heal.UpgradeValueBy(20);
    }

    protected override async Task ApplyDishEffect(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Allies,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<MaxHpPower>(choiceContext, Allies,
            DynamicVars[nameof(MaxHpPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Allies,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Allies,
            DynamicVars[nameof(CriticalDamagePower)].BaseValue, Owner.Creature, this);
        await GrantNpToAllies(DynamicVars["Np"].BaseValue);

        foreach (var ally in Allies)
            await CreatureCmd.Heal(ally, DynamicVars.Heal.BaseValue);
    }
}
