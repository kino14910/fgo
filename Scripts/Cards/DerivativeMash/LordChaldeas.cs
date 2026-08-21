using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     构筑希望的人理之盾: LordCamelot(已然遥远的理想之城)升级获得
/// </summary>
public class LordChaldeas() : NobleCardModel(1, CardType.Power, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<ReducePercentDamagePower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<NpCardPower>(),
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<ArtifactPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Block(15),
        ModCardVars.Power<ReducePercentDamagePower>(30),
        ModCardVars.Power<NpDamagePower>(50),
        ModCardVars.Power<StrengthPower>(3),
        ModCardVars.Power<ArtifactPower>(1)
    ];
    
    public override bool GainsBlock => true;

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, DynamicVars[nameof(ArtifactPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature, DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
        var power = await PowerCmd.Apply<NpCardPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        if (power != null)
            power.NobleCard = ModelDb.Card<RayProofKyrielight>();

        // 时为朦胧的白垩之壁A 变为 测定时间的紫弹之薪
        var chalk = Owner.PlayerCombatState!.AllCards.FirstOrDefault(card => card is ObscurantWallOfChalkA);
        if (chalk is not null && CombatState is not null)
            await CardCmd.Transform(CombatState.CreateCard(chalk, Owner),
                ModelDb.Card<TimewornBulletKindling>().ToMutable());
    }
}