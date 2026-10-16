using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Powers;

public class BlessingOfKurPower : FgoPowerModel
{
    public const int MaxHpPerStack = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("MaxHp", MaxHpPerStack)
    ];

    public decimal MaxHp
    {
        get => DynamicVars["MaxHp"].BaseValue;
        private set
        {
            DynamicVars["MaxHp"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        MaxHp = amount * MaxHpPerStack;
        return base.BeforeApplied(target, amount, applier, cardSource);
    }

    public override Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this)
            MaxHp = (Amount + amount) * MaxHpPerStack;

        return base.BeforePowerAmountChanged(power, amount, target, applier, cardSource);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner) return;
        if (Owner.Player is null) return;

        if (cardPlay.Card is KurKigalIrkalla)
        {
            Flash();
            await PowerCmd.Apply<MaxHpPower>(context, Owner, Amount * MaxHpPerStack, Owner, null);
            await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}