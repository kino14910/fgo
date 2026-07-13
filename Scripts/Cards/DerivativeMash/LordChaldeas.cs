using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class LordChaldeas(): NobleCardModel(1, CardType.Power, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("DamageReduction", 30),
        new PowerVar<PlatingPower>(5)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["DamageReduction"].UpgradeValueBy(20);
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars["DamageReduction"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature, 30m, Owner.Creature, this);
        await PowerCmd.Apply<ForcedNpCardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        // 自身变为印证希望的人理之剑
        await CardCmd.Transform(this, ModelDb.Card<LordChaldeasAtlas>().ToMutable(), CardPreviewStyle.None);

        // 时为朦胧的白垩之壁变为测定时间的紫弹之薪
        var chalk = Owner.PlayerCombatState!.Hand.Cards
            .FirstOrDefault(card => card is ObscurantWallofChalk);
        if (chalk != null)
            await CardCmd.Transform(chalk, ModelDb.Card<TimewornBulletKindling>().ToMutable(), CardPreviewStyle.None);
    }
}