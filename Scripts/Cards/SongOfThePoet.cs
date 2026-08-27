using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class SongOfThePoet() : FgoCardModel(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Damage(6)];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await using var context = await AttackCommand.CreateContextAsync(CombatState!, choiceContext, cardPlay);

        var results = (await CreatureCmd.Damage(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            ValueProp.Move,
            this,
            cardPlay
        )).ToList();

        context.AddHit(results);

        await FgoResCmd.ModifyNp(results.FirstOrDefault()?.UnblockedDamage ?? 0, cardPlay.Player);
    }
}