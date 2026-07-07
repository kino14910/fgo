using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class TimewornBulletKindling : FgoColorlessCard
{
    public TimewornBulletKindling() : base(1, CardType.Attack,
        CardRarity.Token, TargetType.Self)
    {
        WithPower<NpDamagePower>(30);
        WithNp(30, 20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpDamagePower).Name].BaseValue, Owner.Creature, this);
        var stars = ModelDb.Singleton<FgoPlayerResources>().Stars;
        if (stars > 0)
        {
            await FgoStarCmd.ConsumeStars(stars);
            await FgoNpCmd.AddNp(stars * 4);
        }

        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 4m,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
        await CardCmd.Transform(this, ModelDb.Card<ObscurantWallofChalk>().ToMutable(), CardPreviewStyle.None);
    }
}