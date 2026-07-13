using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class TimewornBulletKindling() : ModCardTemplate(1, CardType.Attack,
        CardRarity.Token, TargetType.Self)
    {

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NpDamagePower>(30),
        ModCardVars.Int("Np", 30)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
        var stars = ModelDb.Singleton<FgoPlayerResources>().Stars;
        if (stars > 0)
        {
            await FgoStarCmd.ConsumeStars(stars);
            await FgoNpCmd.AddNp(stars * 4);
        }

        await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 4m,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
        await CardCmd.Transform(this, ModelDb.Card<ObscurantWallofChalk>().ToMutable(), CardPreviewStyle.None);
    }
}