using Fgo.Scripts.Cards.Colorless.EventCards;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Fgo.Scripts.Relics;

/// <summary>
///     [II]: 好感度系统的唯一载体。持有该遗物时打出女神的砂糖才会累积好感度；
///     战斗开始时清空（持有星剑的墓志铭时默认为 10）。
/// </summary>
public class II : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;

    public int Affection { get; private set; }

    public int CardsPlayedThisTurn { get; private set; }

    public override int DisplayAmount => Owner != null ? Affection : 0;

    public int ModifyAffection(int amount)
    {
        var old = Affection;
        Affection = Math.Max(0, Affection + amount);
        if (Affection == old) return 0;
        InvokeDisplayAmountChanged();
        return Affection - old;
    }

    // 基础 +3；本回合第一张卡 +2；〔水边〕场地（WatersidePower）时 +10。
    public void OnBeforeCardPlayed(CardPlay cardPlay)
    {
        if (Owner is not { } owner)
            return;
        if (cardPlay.Card is not { } card)
            return;

        CardsPlayedThisTurn++;

        if (card is GoddessSugar)
        {
            var gain = 3;
            if (CardsPlayedThisTurn == 1) gain += 2;
            if (owner.Creature.HasPower<WatersidePower>()) gain += 10;
            ModifyAffection(gain);
        }
    }

    public void OnAfterPlayerTurnStart()
    {
        CardsPlayedThisTurn = 0;
    }

    public void ResetAffectionForCombat()
    {
        var defaultValue = Owner != null && Owner.GetRelic<AstralSwordEpitaph>() != null ? 10 : 0;
        CardsPlayedThisTurn = 0;
        if (Affection == defaultValue) return;
        Affection = defaultValue;
        InvokeDisplayAmountChanged();
    }
}