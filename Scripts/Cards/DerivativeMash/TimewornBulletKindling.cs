using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     测定时间的紫弹之薪: LordChaldeas(构筑希望的人理之盾)发动后获得
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class TimewornBulletKindling() : FgoBaseCardModel(1, CardType.Attack,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<ObscurantWallOfChalk>(),
        HoverTipFactory.FromPower<NpDamagePower>(),
        FgoHoverTipHelper.CreateNpHoverTip(),
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NpDamagePower>(30),
        ModCardVars.Int("Np", 30)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
        var stars = FgoBattleHooks.Get(Owner).Stars;
        if (stars > 0)
        {
            await FgoResCmd.ModifyStars(-50, Owner);
            await FgoResCmd.ModifyNp(Math.Min(50, stars) * 4, cardPlay.Player);
        }

        await FgoResCmd.ModifyNp(this);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 4m,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
        var wall = CombatState!.CreateCard<ObscurantWallOfChalk>(Owner);
        if (wall is FgoCooldownCardModel wallCd)
            wallCd.ReadyCooldown();
        NCombatRoom.Instance?.Ui.CardPreviewContainer
            ?.AddChildSafely(NCardTransformVfx.Create(this, wall, null));
        await CardPileCmd.AddGeneratedCardToCombat(wall, PileType.Discard, wall.Owner);
    }

    protected override CardLocation GetResultLocationForCardPlay() =>
        new(Owner, PileType.None, CardPilePosition.Bottom);
}