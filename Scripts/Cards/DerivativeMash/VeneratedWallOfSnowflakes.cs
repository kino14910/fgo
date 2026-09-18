using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     荣光坚毅的雪花之壁: WallOfSnowflakes(现为脆弱的雪花之壁)升级获得
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class VeneratedWallOfSnowflakes() : FgoBaseCardModel(1, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ReducePercentDamagePower>(),
        FgoHoverTipFactory.FromNp()
    ];

    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public override CardAssetProfile AssetProfile => new(
        "res://Fgo/images/cards/ObscurantWallOfChalk.png",
        ResourceLoader.Exists("res://Fgo/images/cards/beta/ObscurantWallOfChalk.png")
            ? "res://Fgo/images/cards/beta/ObscurantWallOfChalk.png"
            : null,
        Type switch
        {
            CardType.Attack => "res://Fgo/images/card_frames/card_frame_attack.png",
            CardType.Skill => "res://Fgo/images/card_frames/card_frame_skill.png",
            CardType.Power => "res://Fgo/images/card_frames/card_frame_power.png",
            _ => "res://Fgo/images/card_frames/card_frame_skill.png"
        }
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Block(10),
        ModCardVars.Power<ReducePercentDamagePower>(20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
    }
}