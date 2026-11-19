using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     领袖气质（Charisma）。打出授予 <see cref="CharismaPower" />，并按本卡累计疲劳做一次灾厄判定：
///     疲劳大于当前生命值时施加等于疲劳层数的[gold]灾厄[/gold]。疲劳只由该能力产生，打出本卡自身不加疲劳。
/// </summary>
public class Charisma() : FgoCardModel(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    private const string FatigueKey = "Fatigue";

    private int _fatigue;

    /// <summary>本卡累计的疲劳层数（永久值，随存档持久化），写入时同步卡面显示。</summary>
    [SavedProperty]
    public int CurrentFatigue
    {
        get => _fatigue;
        set
        {
            AssertMutable();
            _fatigue = value;
            DynamicVars[FatigueKey].BaseValue = _fatigue;
        }
    }

    public int Fatigue => DynamicVars[FatigueKey].IntValue;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipFactory.FromFatigue(),
        HoverTipFactory.FromPower<DoomPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int(FatigueKey, CurrentFatigue)
    ];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override void AfterDowngraded()
    {
        SyncFatigueDisplay();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CharismaPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

        var fatigue = Fatigue;
        if (fatigue > Owner.Creature.CurrentHp)
            await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, fatigue,
                Owner.Creature, this);
    }

    public void GainFatigue(int amount)
    {
        if (amount <= 0) return;
        CurrentFatigue += amount;
    }

    private void SyncFatigueDisplay()
    {
        CurrentFatigue = _fatigue;
    }

    /// <summary>累加玩家所有领袖气质卡的疲劳（去重）。</summary>
    /// <remarks>战斗内实例与 <see cref="CardModel.DeckVersion" /> 指向的主卡组本体都要写，否则疲劳只活到战斗结束。</remarks>
    public static void AddFatigue(Player player, CardModel? playedCard, int amount)
    {
        if (amount <= 0) return;

        var seen = new HashSet<Charisma>();
        foreach (var card in Enumerate(player, playedCard))
            if (seen.Add(card))
                card.GainFatigue(amount);
    }

    private static IEnumerable<Charisma> Enumerate(Player player, CardModel? playedCard)
    {
        if (playedCard is Charisma playing) yield return playing;

        foreach (var card in player.Deck.Cards.OfType<Charisma>())
            yield return card;

        if (player.PlayerCombatState is { } combatState)
            foreach (var card in combatState.AllCards.OfType<Charisma>())
            {
                yield return card;

                if (card.DeckVersion is Charisma master)
                    yield return master;
            }
    }
}