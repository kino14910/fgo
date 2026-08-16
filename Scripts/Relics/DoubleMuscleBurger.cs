using Fgo.Scripts.Cards.Colorless;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

public class DoubleMuscleBurger : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterObtained()
    {
        await CreatureCmd.GainMaxHp(Owner.Creature, 40);
        // AddCursesToDeck 内部会调用 RunState.CreateCard(canonical, owner)，
        // 所以这里必须传 canonical singleton，不能先 ToMutable()。
        await CardPileCmd.AddCursesToDeck(Enumerable.Range(0, 3)
            .Select(_ => ModelDb.Card<Dumuzid>()), Owner);
    }
}