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

/// <summary>
///     构筑希望的人理之盾: LordCamelot(已然遥远的理想之城)升级获得
/// </summary>
public class LordChaldeas() : NobleCardModel(1, CardType.Power, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<ReducePercentDamagePower>(30),
        ModCardVars.Power<PlatingPower>(5),
        ModCardVars.Power<NpCardPower>(5),
        ModCardVars.Power<NpDamagePower>(5),
        ModCardVars.Power<StrengthPower>(5),
        ModCardVars.Power<ArtifactPower>(5)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(20);
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature, 30m, Owner.Creature, this);
        var power = await PowerCmd.Apply<NpCardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
            power.NobleCard = ModelDb.Card<HollowHeartAlbion>();

        // 自身变为印证希望的人理之剑
        await CardCmd.Transform(this, ModelDb.Card<RayProofKyrielight>().ToMutable(), CardPreviewStyle.None);

        // 时为朦胧的白垩之壁变为测定时间的紫弹之薪
        var chalk = Owner.PlayerCombatState!.AllCards.FirstOrDefault(card => card is ObscurantWallofChalk);
        if (chalk != null)
            await CardCmd.Transform(chalk, ModelDb.Card<TimewornBulletKindling>().ToMutable(), CardPreviewStyle.None);
    }
}