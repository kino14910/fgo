using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.Colorless.OptionCards;

[RegisterCard(typeof(TokenCardPool))]
public class RepairSpiritOrigin() : FgoBaseCardModel(-2, CardType.Power,
    CardRarity.Token, TargetType.None);