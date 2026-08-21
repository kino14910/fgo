using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     荣光远扬的雪花之盾: VeneratedWallOfSnowflakes(荣光坚毅的雪花之壁)升级获得
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class VeneratedShieldOfSnowflakes() : FgoBaseCardModel(1, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<CriticalDamagePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Block(10),
        ModCardVars.Int("Np", 10),
        ModCardVars.Power<StrengthPower>(2),
        ModCardVars.Power<CriticalDamagePower>(20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["Np"].UpgradeValueBy(10);
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
        DynamicVars[nameof(CriticalDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await FgoResCmd.ModifyNp(this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature, 30m, Owner.Creature, this);
    }
}