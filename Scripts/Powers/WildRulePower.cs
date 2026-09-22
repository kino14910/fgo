using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class WildRulePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/AtkUpPower.png",
        "res://Fgo/images/powers/big/AtkUpPower.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Heal("Heal", 3m),
        ModCardVars.Power<StrengthPower>(1),
        ModCardVars.Power<VulnerablePower>(1)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner) return;
        if (!cardPlay.Card.Tags.Contains(CardTag.Strike)) return;
        if (Owner.Player is null) return;

        Flash();

        await CreatureCmd.Heal(Owner, DynamicVars["Heal"].BaseValue);

        if (cardPlay.Target is not { IsAlive: true } target) return;

        var strengthBefore = target.GetPowerAmount<StrengthPower>();
        if (strengthBefore <= 0) return;

        await PowerCmd.Apply<StrengthPower>(choiceContext, target,
            -DynamicVars[nameof(StrengthPower)].BaseValue, Owner, cardPlay.Card);

        if (target.GetPowerAmount<StrengthPower>() == strengthBefore) return;

        await PowerCmd.Apply<VulnerablePower>(choiceContext, target,
            DynamicVars[nameof(VulnerablePower)].BaseValue, Owner, cardPlay.Card);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (side != Owner.Side) return;

        await PowerCmd.Decrement(this);
    }
}