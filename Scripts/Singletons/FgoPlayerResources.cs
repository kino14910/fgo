// using Fgo.Scripts.Cards.NoblePhantasm;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Fgo.Scripts.Singletons;

[RegisterSingleton]
public class FgoPlayerResources() : HookedSingletonModel(HookType.Combat)
{
    private const int MaxCommandSpell = 3;
    private const int MaxOverCharge = 4;
    private const int BasicCritStars = 10;
    private const int SpecialCritStars = 20;

    private int _commandSpell = 3;
    private int _mashUpgradeLevel;
    private int _np;
    private bool _npButtonPressed;
    private int _stars;

    public int Np
    {
        get => _np;
        private set => _np = Math.Clamp(value, 0, 300);
    }

    public int Stars
    {
        get => _stars;
        private set => _stars = Math.Max(0, value);
    }

    public int CommandSpell
    {
        get => _commandSpell;
        private set => _commandSpell = Math.Clamp(value, 0, MaxCommandSpell);
    }

    public int MashUpgradeLevel
    {
        get => _mashUpgradeLevel;
        private set => _mashUpgradeLevel = Math.Max(0, value);
    }

    public bool CanCrit => Stars >= BasicCritStars;
    public bool CanUseNp => Np >= 100;
    public bool CanUseCommandSpell => CommandSpell > 0;
    public int OverCharge { get; private set; }

    public int OverChargeLevel => OverCharge;


    /// <summary>
    ///     尝试消耗暴击星。返回实际消耗的数量（0=未触发）。
    ///     special=true 时按 SpecialCritStars(20) 消耗，否则按 BasicCritStars(10)。
    /// </summary>
    public int TryConsumeCritStars(bool special)
    {
        var required = special ? SpecialCritStars : BasicCritStars;
        if (Stars < required) return 0;
        ConsumeStars(required);
        return required;
    }

    public async Task AddNp(int amount, PlayerChoiceContext? choiceContext = null, Player? player = null)
    {
        var old = Np;
        Np += amount;
        if (Np == 99 && old < 99) Np = 100;

        SyncOverCharge(old, Np);

        // FgoCombatUi.UpdateAll();
    }

    public Task<bool> ConsumeNp(int amount, Player? player = null)
    {
        if (Np < amount) return Task.FromResult(false);
        var old = Np;
        Np -= amount;
        SyncOverCharge(old, Np);
        return Task.FromResult(true);
    }

    public void GainOverCharge(int amount)
    {
        if (amount <= 0) return;
        OverCharge = Math.Clamp(OverCharge + amount, 0, MaxOverCharge);
    }

    public void SpendNpForNoblePhantasm()
    {
        _np = 0;
        // FgoCombatUi.UpdateAll();
    }

    public void AddStars(int amount = 1)
    {
        Stars += amount;
        // FgoCombatUi.UpdateAll();
    }

    public bool ConsumeStars(int amount)
    {
        if (Stars < amount) return false;
        Stars -= amount;
        // FgoCombatUi.UpdateAll();
        return true;
    }

    public bool UseCommandSpell(int amount = 1)
    {
        if (CommandSpell < amount) return false;
        CommandSpell -= amount;
        // FgoCombatUi.UpdateAll();
        return true;
    }

    public void ResetCommandSpell()
    {
        CommandSpell = MaxCommandSpell;
    }

    public void Reset()
    {
        _np = 0;
        _stars = 0;
        OverCharge = 0;
        _commandSpell = MaxCommandSpell;
        _npButtonPressed = false;
        // FgoCardActions.ClearAllForcedNpCards();
        // FgoCombatUi.UpdateAll();
    }

    public void SetNpButtonPressed()
    {
        _npButtonPressed = true;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (_npButtonPressed && CanUseNp) _npButtonPressed = false;
    }

    // public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    // {
    //     return card is NobleCardModel ? playCount + OverCharge : playCount;
    // }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
            OverCharge = 0;
        return Task.CompletedTask;
    }

    private static int OverChargeLevelFor(int np)
    {
        return np >= 300 ? 2 : np >= 200 ? 1 : 0;
    }

    private void SyncOverCharge(int oldNp, int newNp)
    {
        var delta = OverChargeLevelFor(newNp) - OverChargeLevelFor(oldNp);
        if (delta > 0)
            GainOverCharge(delta);
        else if (delta < 0)
            OverCharge = Math.Max(0, OverCharge + delta);
    }


    /// <summary>
    ///     马修卡牌强化等级：0=初始, 1=第一层Boss后, 2=第二层Boss后
    /// </summary>
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        var player = LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault(c => c.IsActiveForHooks);
        if (room.RoomType != RoomType.Boss || player == null) return;
        if (MashUpgradeLevel >= 2) return;

        MashUpgradeLevel++;

        // if (MashUpgradeLevel == 1)
        //     // 第一次升级：WallOfSnowflakes -> VeneratedWallOfSnowflakes
        //     foreach (var card in player.Deck.Cards.OfType<WallOfSnowflakes>().ToList())
        //         await CardCmd.Transform(card, ModelDb.Card<VeneratedWallOfSnowflakes>().ToMutable(),
        //             CardPreviewStyle.None);
        // else if (MashUpgradeLevel == 2)
        //     // 第二次升级：VeneratedWallOfSnowflakes -> VeneratedShieldOfSnowflakes
        //     foreach (var card in player.Deck.Cards.OfType<VeneratedWallOfSnowflakes>().ToList())
        //         await CardCmd.Transform(card, ModelDb.Card<VeneratedShieldOfSnowflakes>().ToMutable(),
        //             CardPreviewStyle.None);
    }
}