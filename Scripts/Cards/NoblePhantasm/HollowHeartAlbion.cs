using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class HollowHeartAlbion : NobleCardModel
{
    public HollowHeartAlbion() : base(2, CardType.Attack, TargetType.Self)
    {
        WithVar(new DamageVar(27m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move));
        WithStar(10, 4);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, CombatState!.HittableEnemies,
            DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature);
        await FgoStarCmd.AddStars(DynamicVars.Stars.IntValue);
    }
}