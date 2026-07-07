using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Events;

[RegisterSharedEvent]
public class ManofChaldea : FgoEventModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(75),
        new HealVar(15)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            new EventOption(this, Continue, InitialOptionKey("CONTINUE"))
        ];
    }

    public async Task Continue()
    {
        SetEventState(PageDescription("PAGE0"),
        [
            new EventOption(this, Continue2, ModOptionKey("PAGE0", "CONTINUE2"))
        ]);
    }

    public async Task Continue2()
    {
        SetEventState(PageDescription("PAGE1"),
        [
            new EventOption(this, Gold, ModOptionKey("PAGE1", "GOLD")),
            new EventOption(this, Heal, ModOptionKey("PAGE1", "HEAL"))
        ]);
    }

    public async Task Gold()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.IntValue, Owner!);
        SetEventFinished(PageDescription("GOLD"));
    }

    public async Task Heal()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.BaseValue);
        SetEventFinished(PageDescription("HEAL"));
    }
}