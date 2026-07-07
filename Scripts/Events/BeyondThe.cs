using Fgo.Scripts.Cards.Colorless;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Events;

[RegisterSharedEvent]
public class BeyondThe : FgoEventModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("MaxHpgain", 6m)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            new EventOption(this, Continue, InitialOptionKey("CONTINUE")),
            new EventOption(this, Shvibzik, ModOptionKey("PHASE_1", "SHVIBZIK")),
            new EventOption(this, MaxHp, ModOptionKey("PHASE_1", "MAXHP"))
        ];
    }

    public async Task Continue()
    {
        SetEventState(PageDescription("PHASE_1"),
        [
            new EventOption(this, Shvibzik, ModOptionKey("PHASE_1", "SHVIBZIK")),
            new EventOption(this, MaxHp, ModOptionKey("PHASE_1", "MAXHP"))
        ]);
    }

    public async Task Shvibzik()
    {
        var card = ModelDb.Card<Shvibzik>().ToMutable();
        await CardPileCmd.Add(card, PileType.Hand);
        SetEventFinished(PageDescription("LEAVE"));
    }

    public async Task MaxHp()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["MaxHpgain"].IntValue);
        SetEventFinished(PageDescription("LEAVE"));
    }
}