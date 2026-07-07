using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Utils;

[RegisterOwnedCardTag(nameof(Foreigner))]
public class FgoTags
{
    public static readonly CardTag Foreigner =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Foreigner)).GetModCardTag();
}