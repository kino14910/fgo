using Fgo.Scripts.Character;
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
            .RegisterOwned("Noble", new ModCardPileSpec
            {
                // RunPersistent: 绑定到 Player 而非 PlayerCombatState，跨战斗保留并随存档保存。
                Scope = ModCardPileScope.RunPersistent,
                // 显示在顶栏 deck 按钮旁。
                Style = ModCardPileUiStyle.TopBarDeck,
                IconPath = $"res://{modId}/images/ui/noble_deck_button.png",
                VisibleWhen = ctx =>
                    ctx.Player is null || ctx.Player.Character is FgoCharacter
            }).PileType;
        NoblePhantasm = RitsuLibFramework.RegisterDynamicEnumValue<CardRarity>(modId, "NoblePhantasm");
    }
}