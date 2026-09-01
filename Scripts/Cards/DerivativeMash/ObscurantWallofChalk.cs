using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     时为朦胧的白垩之壁:初始卡
/// </summary>
[RegisterCharacterStarterCard(typeof(FgoCharacter))]
public class ObscurantWallOfChalk() : FgoCooldownCardModel(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override int CooldownMax => 7;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<BufferPower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> AdditionalCanonicalVars =>
    [
        ModCardVars.Int("Np", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BufferPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await FgoResCmd.ModifyNp(this);
    }
}