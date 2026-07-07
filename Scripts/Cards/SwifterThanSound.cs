using Fgo.Scripts.Cards.Colorless;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Cards;

public class SwifterThanSound : FgoCardModel
{
    public SwifterThanSound() : base(1, CardType.Attack,
        CardRarity.Rare, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithDamage(6, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PlayerCmd.GainEnergy(1m, Owner);
        await FgoCardActions.AddToPile(ModelDb.Card<InfiniteSuffering>().ToMutable(), PileType.Hand);
    }
}