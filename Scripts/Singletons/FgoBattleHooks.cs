using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Relics;
using Fgo.Scripts.UI;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2RitsuLib.Utils;

namespace Fgo.Scripts.Singletons;

/// <summary>
///     FGO 战斗钩子中枢（全 run 单例）: 接收官方 combat hooks，按玩家路由到对应
///     <see cref="FgoPlayerState" />，并以静态 <see cref="Get" /> 提供按玩家的状态存储。
///     多人模式下状态须按 Player 分实例存放，单例自身只承担接收与路由。
/// </summary>
[RegisterSingleton]
public sealed class FgoBattleHooks() : HookedSingletonModel(HookType.Combat)
{
    private static readonly AttachedState<Player, FgoPlayerState> States = new(() => new FgoPlayerState());

    /// <summary>
    ///     获取指定玩家的战斗资源状态（不存在则创建）。
    /// </summary>
    public static FgoPlayerState Get(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return States.GetOrCreate(player);
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card?.Owner is not { Character: FgoCharacter } player)
            return;
        await Get(player).OnBeforeCardPlayed(cardPlay);
        player.GetRelic<II>()?.OnBeforeCardPlayed(cardPlay);
    }

    public override async Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker?.Player is not { Character: FgoCharacter } player)
            return;
        await Get(player).OnBeforeAttack(command);
    }

    /// <summary>枚举指定玩家分布在各牌堆中的所有冷却卡实例，用于统一重置/遍历。</summary>
    private static IEnumerable<FgoCooldownCardModel> CooldownCards(Player player)
    {
        foreach (var pile in Enum.GetValues<PileType>())
        {
            if (pile == PileType.None) continue;
            foreach (var card in pile.GetPile(player).Cards.OfType<FgoCooldownCardModel>())
                yield return card;
        }
    }

    public override async Task BeforeCombatStart()
    {
        FgoGlobalHud.WakeInstances();

        var combat = CurrentCombatState;
        if (combat == null) return;
        foreach (var player in combat.Players)
        {
            if (player.Character is not FgoCharacter)
                continue;
            await Get(player).Reset();
            // 好感度由 II 遗物重置: 默认 0，持星剑的墓志铭时 10
            player.GetRelic<II>()?.ResetAffectionForCombat();

            foreach (var cd in CooldownCards(player))
                cd.ReadyCooldown();
        }
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer?.Player is not { Character: FgoCharacter } player)
            return 1m;
        return Get(player).ModifyDamageMultiplicative(target, amount, props, dealer, cardSource,
            cardPlay);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card?.Owner is not { Character: FgoCharacter } player)
            return;

        // 打出任意宝具牌后，为该玩家获得 OverchargePower 层数（最多 MaxOvercharge 层）。
        if (cardPlay.Card is NobleCardModel)
        {
            var existing = player.Creature.GetPower<OverchargePower>();
            var current = existing?.Amount ?? 0;
            var delta = Math.Min(OverchargePower.MaxOvercharge, current + 1) - current;
            if (delta > 0)
                await PowerCmd.Apply<OverchargePower>(
                    choiceContext, player.Creature, delta, player.Creature, null);
        }

        Get(player).ResetCrit();

        // 冷却机制：
        // 1) 打出一张冷却卡 → 其冷却重置为 CooldownMax（重新进入冷却）；
        // 2) 打出任意一张牌 → 玩家手牌中其余冷却卡冷却 -1（到时为 0 即可打出）。
        var playedCooldown = cardPlay.Card as FgoCooldownCardModel;
        playedCooldown?.ResetCooldown();

        foreach (var cd in PileType.Hand.GetPile(player).Cards.OfType<FgoCooldownCardModel>())
            if (cd != playedCooldown && cd.CurrentCooldown > 0)
                cd.DecrementCooldown();
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Character is not FgoCharacter)
            return Task.CompletedTask;
        Get(player).OnAfterPlayerTurnStart();
        player.GetRelic<II>()?.OnAfterPlayerTurnStart();
        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer?.Player is not { Character: FgoCharacter } player
            || cardSource is not FgoCardModel { Type: CardType.Attack }
            || result.TotalDamage <= 0) return;

        await Get(player).ModifyStars(1, player);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target is { IsPlayer: true, Player.Character: FgoCharacter }
            && dealer is { IsMonster: true }
            && result.TotalDamage > 0
            && props.IsPoweredAttack())
            await FgoResCmd.ModifyNp(result.TotalDamage, target.Player);
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature.Player is not { Character: FgoCharacter } player)
            return true;
        return Get(player).ShouldDie(creature);
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature.Player is not { Character: FgoCharacter } player)
            return;
        await Get(player).AfterPreventingDeath(creature);
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null) return;

        foreach (var player in state.Players.Where(p => p.Character is FgoCharacter))
        {
            var playerState = Get(player);
            await playerState.SaveCommandSpellToRunState(player);

            if (room.RoomType == RoomType.Boss && playerState.MashUpgradeLevel < 2)
                playerState.MashUpgradeLevel++;
        }
    }
}