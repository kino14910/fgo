using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

[RegisterRelic(typeof(DeprecatedRelicPool))]
public class Salem : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var foreignerCount = Owner.Deck.Cards.Count(card => card.Tags.Contains(FgoTags.Foreigner));
        if (foreignerCount > 0)
        {
            Flash();
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, foreignerCount * 2, Owner.Creature, null);
            await FgoResCmd.ModifyStars(foreignerCount * 2, Owner.Creature.Player);
        }
    }
}