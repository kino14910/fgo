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

    /// <summary>
    ///     OverchargePower 层数上限的统一收口。
    ///     <para>
    ///         `PowerModel.Amount` 的 getter 不是 virtual（内部 `_amount` + `SetAmount`），无法靠重写 getter 封顶；
    ///         唯一对显示开放的 virtual 是 `DisplayAmount`，但它只改角标数字、不改真实层数。
    ///         因此上限必须拦在写入处：官方钩子 `TryModifyPowerAmountReceived` 是 PowerCmd 两条写入路径
    ///         （首次施加 `Apply`、已存在时叠加 `ModifyAmount`）在 `SetAmount` 之前都会调用的修正点，
    ///         返回 true 即用 `modifiedAmount` 覆盖本次增量。
    ///     </para>
    ///     这里把「当前层数 + 增量」钳进 [.., MaxOvercharge]：任何来源（卡牌/遗物/怪物）都无法把层数推过上限；
    ///     顺带把历史存档里可能超上限的层数在下次变动时归一化回上限。
    /// </summary>
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount,
        Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (canonicalPower is not OverchargePower) return false;

        var current = target.GetPower<OverchargePower>()?.Amount ?? 0;
        var cappedDelta = Math.Min(OverchargePower.MaxOvercharge, current + (int)amount) - current;

        // 未触及上限时不介入，避免抢掉其它修正逻辑。
        if (cappedDelta == amount) return false;

        modifiedAmount = cappedDelta;
        return true;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card?.Owner is not { Character: FgoCharacter } player)
            return;

        // 打出任意宝具牌后，为该玩家获得 1 层 OverchargePower；
        // 层数上限由本类的 TryModifyPowerAmountReceived 统一封顶（满层时增量为 0，Apply 自动跳过）。
        if (cardPlay.Card is NobleCardModel)
            await PowerCmd.Apply<OverchargePower>(choiceContext, player.Creature, 1, player.Creature, null);

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