using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Sevendrive : NobleCardModel
{
    public Sevendrive() : base(3, CardType.Attack, TargetType.Self)
    {
        WithDamage(12, 4);
        WithVar("Strength", 2);
        WithNp(20);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // 获得2临时力量（回合结束自动失去）
        await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, Owner.Creature, DynamicVars["Strength"].BaseValue,
            Owner.Creature, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}