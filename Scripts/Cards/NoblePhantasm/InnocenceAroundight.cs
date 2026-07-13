using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class InnocenceAroundight(): NobleCardModel(2, CardType.Attack, TargetType.AnyEnemy) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(32, ValueProp.Move),
        new PowerVar<NpRatePower>(3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpRatePower)].BaseValue, Owner.Creature, this);
        await FgoCardActions.AddToPile(FgoCardActions.CreateCard<RayHorizon>(true), PileType.Draw);
    }
}