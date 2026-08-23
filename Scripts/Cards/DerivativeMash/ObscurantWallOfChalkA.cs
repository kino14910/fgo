using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using Godot;
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

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     时为朦胧的白垩之壁A: ObscurantWallOfChalk(时为朦胧的白垩之壁)升级获得
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class ObscurantWallOfChalkA() : FgoCooldownCardModel(1, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    public override int CooldownMax => 8;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AntiPurgeDefensePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    /// <summary>
    ///     复用初始形态 ObscurantWallOfChalk 的卡图（本项目未提供单独的 ObscurantWallOfChalkA 卡图）。
    /// </summary>
    public override CardAssetProfile AssetProfile => new(
        "res://Fgo/images/cards/big/ObscurantWallOfChalk.png",
        "res://Fgo/images/cards/big/beta/ObscurantWallOfChalk.png",
        Type switch
        {
            CardType.Attack => "res://Fgo/images/card_frames/card_frame_attack.png",
            CardType.Skill => "res://Fgo/images/card_frames/card_frame_skill.png",
            CardType.Power => "res://Fgo/images/card_frames/card_frame_power.png",
            _ => "res://Fgo/images/card_frames/card_frame_skill.png"
        }
    );

    protected override IEnumerable<DynamicVar> CoreCanonicalVars =>
    [
        ModCardVars.Int("Np", 20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AntiPurgeDefensePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await FgoResCmd.ModifyNp(this);
    }
}