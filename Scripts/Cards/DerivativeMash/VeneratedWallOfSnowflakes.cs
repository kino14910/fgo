using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     荣光坚毅的雪花之壁: WallOfSnowflakes(现为脆弱的雪花之壁)升级获得
/// </summary>
public class VeneratedWallOfSnowflakes() : FgoColorlessCardModel(1, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ReducePercentDamagePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Block(10),
        ModCardVars.Power<ReducePercentDamagePower>(20)
    ];


    public override CardAssetProfile AssetProfile => new(
        "res://Fgo/images/cards/big/ObscurantWallOfChalk.png",
        ResourceLoader.Exists("res://Fgo/images/cards/big/beta/ObscurantWallOfChalk.png")
            ? "res://Fgo/images/cards/big/beta/ObscurantWallOfChalk.png"
            : null,
        Type switch
        {
            CardType.Attack => "res://Fgo/images/card_frames/card_frame_attack.png",
            CardType.Skill => "res://Fgo/images/card_frames/card_frame_skill.png",
            CardType.Power => "res://Fgo/images/card_frames/card_frame_power.png",
            _ => "res://Fgo/images/card_frames/card_frame_skill.png"
        }
    );

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
    }
}