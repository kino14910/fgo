using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib;
using STS2RitsuLib.CardPiles;

namespace Fgo.Scripts.Utils;

public static class FgoEnums
{
    public static PileType NobleDeck { get; private set; }

    public static CardRarity NoblePhantasm { get; private set; }

    public static void Initialize(string modId)
    {
        NobleDeck = ModCardPileRegistry.For(modId)
            .RegisterOwned("NobleDeck", new ModCardPileSpec
            {
                Scope = ModCardPileScope.CombatOnly,
                Style = ModCardPileUiStyle.Headless
            }).PileType;
        NoblePhantasm = RitsuLibFramework.RegisterDynamicEnumValue<CardRarity>(modId, "NoblePhantasm");
    }
}