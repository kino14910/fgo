using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.Colorless;

public class InfiniteSuffering : FgoColorlessCard
{
    public InfiniteSuffering() : base(2, CardType.Attack,
        CardRarity.Token, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithDamage(8, 2);
        WithPower<VulnerablePower>(2, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars[typeof(VulnerablePower).Name].BaseValue, Owner.Creature, this);
        await FgoCardActions.AddToPile(ModelDb.Card<TheAbsoluteSword>().ToMutable(), PileType.Hand);
    }
}