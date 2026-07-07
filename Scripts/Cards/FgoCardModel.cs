using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

[RegisterCard(typeof(FgoCardPool), Inherit = true)]
public abstract class FgoCardModel(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : FgoCardBase(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    /// <summary>
    ///     默认 AssetProfile：根据 CardType 选择卡框；派生类可重写。
    /// </summary>
    public override CardAssetProfile AssetProfile => new(
        $"res://Fgo/images/cards/big/{GetType().Name}.png",
        ResourceLoader.Exists($"res://Fgo/images/cards/big/beta/{GetType().Name}.png")
            ? $"res://Fgo/images/cards/big/beta/{GetType().Name}.png"
            : null,
        type switch
        {
            CardType.Attack => "res://Fgo/images/card_frames/card_frame_attack.png",
            CardType.Skill => "res://Fgo/images/card_frames/card_frame_skill.png",
            CardType.Power => "res://Fgo/images/card_frames/card_frame_power.png",
            _ => "res://Fgo/images/card_frames/card_frame.png"
        }
    );

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return FgoNpCmd.AddNp(cardPlay.Card.EnergyCost.Canonical);
    }
}