using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

public class MoonRabbit() : FgoCardModel(2, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    private const int UpgradeHealBonus = 3;

    public override CardAssetProfile AssetProfile =>
        new("res://Fgo/images/cards/DancingUnderTheFullMoon.png", null,
            "res://Fgo/images/card_frames/card_frame_skill.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<CriticalDamagePower>(30),
        ModCardVars.Heal(5)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(UpgradeHealBonus);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, creature,
            DynamicVars[nameof(CriticalDamagePower)].BaseValue, creature, this);
        await CreatureCmd.Heal(creature, DynamicVars["Heal"].BaseValue);
    }
}