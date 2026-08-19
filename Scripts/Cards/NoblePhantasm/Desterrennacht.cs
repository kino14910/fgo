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

public class Desterrennacht() : NobleCardModel(3, CardType.Power, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StarsPerTurnPower>(),
        HoverTipFactory.FromPower<CriticalDamagePower>(),
        HoverTipFactory.FromPower<ForeignerCriticalDamagePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<TerrorPower>(),
        HoverTipFactory.FromPower<StarsPerTurnPower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<StrengthPower>(2),
        ModCardVars.Power<CriticalDamagePower>(60),
        ModCardVars.Power<ForeignerCriticalDamagePower>(60),
        ModCardVars.Power<TerrorPower>(3),
        ModCardVars.Power<StarsPerTurnPower>(10),
        ModCardVars.Int("TerrorChance", 60)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
        DynamicVars[nameof(CriticalDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var terror = await PowerCmd.Apply<TerrorPower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(TerrorPower)].IntValue,
            Owner.Creature, this);

        terror.FirstOrDefault()!.TerrorChance = DynamicVars["TerrorChance"].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(CriticalDamagePower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<ForeignerCriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ForeignerCriticalDamagePower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<StarsPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StarsPerTurnPower)].BaseValue,
            Owner.Creature, this);
    }
}