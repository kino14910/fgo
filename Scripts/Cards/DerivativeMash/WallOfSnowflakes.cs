using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class WallOfSnowflakes : FgoCardModel
{
    private int _cachedMaxLevel;

    public WallOfSnowflakes() : base(1, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        WithBlock(7, 3);
        WithKeyword(CardKeyword.Eternal);
    }

    private bool BypassUpgradeCheck { get; set; }
    public override int MaxUpgradeLevel => BypassUpgradeCheck ? _cachedMaxLevel : 0;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    public void ForceUpgrade()
    {
        BypassUpgradeCheck = true;
        _cachedMaxLevel++;
        UpgradeInternal();
        FinalizeUpgradeInternal();
        BypassUpgradeCheck = false;
    }
}