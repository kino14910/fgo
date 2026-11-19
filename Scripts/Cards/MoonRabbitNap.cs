using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

// 只作为 MoonRabbit 的右键替换目标存在，不进奖励卡池与图鉴。
[RegisterCard(typeof(TokenCardPool))]
public class MoonRabbitNap() : FgoCardModel(2, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self, false)
{
    private const int UpgradeNpDamageBonus = 10;

    public override CardAssetProfile AssetProfile =>
        new("res://Fgo/images/cards/NappingUnderTheSun.png", null,
            "res://Fgo/images/card_frames/card_frame_skill.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<BufferPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NpDamagePower>(20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NpDamagePower)].UpgradeValueBy(UpgradeNpDamageBonus);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<NpDamagePower>(choiceContext, creature,
            DynamicVars[nameof(NpDamagePower)].BaseValue, creature, this);
        await PowerCmd.Apply<BufferPower>(choiceContext, creature, 1m, creature, this);
    }
}