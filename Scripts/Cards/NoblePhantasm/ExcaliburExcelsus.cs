using Fgo.Scripts.Commands;
using Fgo.Scripts.Keywords;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class ExcaliburExcelsus() : NobleCardModel(3, CardType.Attack, TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, FgoKeywords.IgnoreInvincible];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(16),
        ModCardVars.Block(6),
        ModCardVars.Power<StrengthPower>(1),
        ModCardVars.Int("Np", 10)
    ];

    public override bool GainsBlock => true;

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IgnoreInvincibleAction(CombatState!.HittableEnemies);

        var enemyCount = CombatState!.HittableEnemies.Count;
        // 每有一名敌人，获得格挡、力量、宝具值
        if (enemyCount > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue * enemyCount,
                ValueProp.Unpowered, cardPlay);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
                DynamicVars.Strength.BaseValue * enemyCount, Owner.Creature, this);
            await FgoResCmd.ModifyNp(DynamicVars["Np"].IntValue * enemyCount, cardPlay.Player);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}