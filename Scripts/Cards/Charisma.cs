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
///     领袖气质（Charisma）。
///     <para>
///         打出后授予<see cref="CharismaPower" />：持有该能力期间，你在能量不足时也能打出卡牌，
///         每缺少 1 点能量失去 1 点生命并获得 1 层[gold]疲劳[/gold]。
///     </para>
///     <para>
///         疲劳**只**由该能力产生 —— 打出本卡自身不增加疲劳。
///         打出本卡时做一次判定：若本卡的[gold]疲劳[/gold]大于你的当前生命值，
///         则对你施加等于疲劳层数的[gold]灾厄[/gold]。
///     </para>
///     <para>
///         永久持久化沿用原版 TheScythe 的模式：疲劳是卡牌自身的 [SavedProperty]，
///         写入时同步卡面显示值，并经由 <c>DeckVersion</c> 写回主卡组本体，跨战斗保留。
///     </para>
/// </summary>
public class Charisma() : FgoCardModel(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    private const string FatigueKey = "Fatigue";

    private int _fatigue;

    /// <summary>本卡累计的疲劳层数（永久值，随卡牌存档持久化）。写入时同步卡面显示值。</summary>
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

    /// <summary>本卡当前疲劳层数。</summary>
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
        // 升级只降低费用；能力与生命损失不受升级影响。
        EnergyCost.UpgradeBy(-1);
    }

    protected override void AfterDowngraded()
    {
        // 疲劳是卡牌自身的持久值，降级只需把卡面显示值重新同步回来。
        SyncFatigueDisplay();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 授予能力: 持有期间能量不足也可打出卡牌（缺口由能力结算为生命 + 疲劳）。
        await PowerCmd.Apply<CharismaPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

        // 本卡自身打出不加疲劳；只做一次灾厄判定。
        var fatigue = Fatigue;
        if (fatigue > Owner.Creature.CurrentHp)
            await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, fatigue,
                Owner.Creature, this);
    }

    /// <summary>为本卡累加疲劳（永久值），并刷新卡面显示。</summary>
    public void GainFatigue(int amount)
    {
        if (amount <= 0) return;
        CurrentFatigue += amount;
    }

    private void SyncFatigueDisplay()
    {
        CurrentFatigue = _fatigue;
    }

    // ---- 供 CharismaPower 结算能量缺口时使用 ----

    /// <summary>
    ///     为玩家的[gold]领袖气质[/gold]累加疲劳。
    ///     <para>
    ///         沿用 TheScythe 的写法：战斗内的实例写完，还要通过 <see cref="CardModel.DeckVersion" />
    ///         把同一个值写回主卡组本体，否则疲劳只活在本次战斗里，战斗结束就丢了。
    ///     </para>
    ///     <para>
    ///         主卡组本体（<c>player.Deck</c>）里的卡在 <c>PopulateStartingDeck</c> 时已被 <c>ToMutable()</c>，
    ///         可以安全地 <see cref="AbstractModel.AssertMutable" />，所以这里直接调用 <see cref="GainFatigue" />。
    ///     </para>
    /// </summary>
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

                // TheScythe 模式：战斗内实例 → 主卡组本体，保证跨战斗持久化。
                if (card.DeckVersion is Charisma master)
                    yield return master;
            }
    }
}
