using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class WallOfSnowflakes() : FgoCardModel(1, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    private int _cachedMaxLevel;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
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