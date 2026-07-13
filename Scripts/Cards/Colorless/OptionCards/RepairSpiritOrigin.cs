using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.Colorless.OptionCards;

[RegisterCard(typeof(TokenCardPool))]
public class RepairSpiritOrigin() : ModCardTemplate(-2, CardType.Power, CardRarity.Token, TargetType.None)
{
    public override CardAssetProfile AssetProfile => new(
        $"res://Fgo/images/cards/big/{GetType().Name}.png",
        ResourceLoader.Exists($"res://Fgo/images/cards/big/beta/{GetType().Name}.png")
            ? $"res://Fgo/images/cards/big/beta/{GetType().Name}.png"
            : null,
        "res://Fgo/images/card_frames/card_frame_power.png");
}