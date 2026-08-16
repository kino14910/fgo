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

namespace Fgo.Scripts.Cards;

public class WitchOfSalem() : FgoCardModel(3, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<TerrorPower>(),
        HoverTipFactory.FromPower<VsTerrorDamagePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<VulnerablePower>(3),
        ModCardVars.Power<WeakPower>(3),
        ModCardVars.Power<TerrorPower>(3),
        ModCardVars.Int("TerrorChance", 30),
        ModCardVars.Power<VsTerrorDamagePower>(50),
        ModCardVars.Int("Np", 20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["TerrorChance"].UpgradeValueBy(20);
        DynamicVars[nameof(VsTerrorDamagePower)].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars[nameof(WeakPower)].BaseValue,
                Owner.Creature, this);
            await TerrorPower.Apply(choiceContext, enemy,
                DynamicVars[nameof(TerrorPower)].IntValue,
                DynamicVars["TerrorChance"].BaseValue,
                Owner.Creature, this);
        }

        await PowerCmd.Apply<VsTerrorDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(VsTerrorDamagePower)].BaseValue, Owner.Creature, this);
        await FgoResCmd.ModifyNp(this);
    }
}