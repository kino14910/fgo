using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
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
        HoverTipFactory.FromPower<ReducePercentDamagePower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<ArtifactPower>(),
        HoverTipFactory.FromPower<NpCardPower>(),
        HoverTipFactory.FromCard<RayProofKyrielight>(),
        HoverTipFactory.FromCard<ObscurantWallOfChalkA>(),
        HoverTipFactory.FromCard<TimewornBulletKindling>(),
        FgoHoverTipHelper.CreateKnightOfChaldeaHoverTip(),
        FgoHoverTipHelper.CreateNpHoverTip(),
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<PlatingPower>(6),
        ModCardVars.Power<ReducePercentDamagePower>(30),
        ModCardVars.Power<NpDamagePower>(50),
        ModCardVars.Power<StrengthPower>(3),
        ModCardVars.Power<ArtifactPower>(1)
    ];

    public override bool GainsBlock => false;

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(1);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 除 NP 相关外，其余能力（ReducePercentDamage/Plating/Strength/Artifact）改为全体获得。
        var allies = CombatState!.GetTeammatesOf(Owner.Creature);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, allies,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, allies,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, allies,
            DynamicVars[nameof(ArtifactPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, allies,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);

        // NpDamagePower：只给同队中属于 FGO 角色的友方。
        var fgoAllies = CombatState!.GetTeammatesOf(Owner.Creature)
            .Where(c => c.Player?.Character is FgoCharacter);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, fgoAllies,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);

        // NpCard 只作用于打出者自己。
        var power = await PowerCmd.Apply<NpCardPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        if (power != null)
            power.NobleCard = ModelDb.Card<RayProofKyrielight>();

        // 时为朦胧的白垩之壁A 变为 测定时间的紫弹之薪
        var chalk = Owner.PlayerCombatState!.AllCards
            .FirstOrDefault(card => card is ObscurantWallOfChalkA);
        if (chalk is not null)
            await CardCmd.TransformTo<TimewornBulletKindling>(chalk);
    }
}