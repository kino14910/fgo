using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     印证希望的人理之剑：LordChaldeas 变身后的攻击形态。
/// </summary>
public class LordChaldeasAtlas : NobleCardModel
{
    public LordChaldeasAtlas() : base(1, CardType.Attack,
        TargetType.AnyEnemy)
    {
        WithDamage(20, 10);
        WithPower<NpDamagePower>(30);
        WithNp(30, 20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpDamagePower).Name].BaseValue, Owner.Creature, this);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}