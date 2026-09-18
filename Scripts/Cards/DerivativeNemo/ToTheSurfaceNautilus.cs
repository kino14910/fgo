using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Fgo.Scripts.Cards.DerivativeNemo;

/// <summary>
///     上浮吧鹦鹉螺号: 对一名敌人打出<b>鹦鹉螺的大冲角</b>；若斩杀该敌人，获得 2 点探索点数。
/// </summary>
/// <remarks>
///     斩杀判定与官方 <c>Feed</c> 同源: 先用「所有能力都允许其死亡触发斩杀」做门槛，再确认目标确实死了。
///     <c>CardCmd.AutoPlay</c> 拿不到 <c>AttackCommand.Results</c>，故改用「攻击前存活 → 攻击后不存活」等价判定
///     （<c>Creature.IsAlive</c> = <c>CurrentHp &gt; 0</c>，被「毅力」救回仍算存活，
///     比 <c>WasTargetKilled</c> 的"血量触底"更贴合「斩杀」含义）。
/// </remarks>
public class ToTheSurfaceNautilus() : FgoCardModel(0, CardType.Power, 
    CardRarity.Token, TargetType.AnyEnemy,
    shouldShowInCardLibrary: false)
{
    /// <summary>斩杀时获得的探索点数。</summary>
    private const decimal FatalExplorePoints = 2m;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<GreatRamNautilus>(),
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        FgoHoverTipFactory.FromExploration()
    ];
    
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override bool IsPlayable => FgoBattleHooks.Get(Owner).Np >= 100;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        await FgoBattleHooks.Get(Owner).ModifyNp(-100, Owner);

        var target = cardPlay.Target;
        var shouldTriggerFatal = target.IsAlive
                                 && target.Powers.All(static p => p.ShouldOwnerDeathTriggerFatal());

        // 战斗中的卡一律走 ICombatState.CreateCard: RunState.CreateCard 只登记进运行存档的卡表，
        // 手动/自动出牌时都会踩 "must be added to a CombatState before playing it."
        var ram = combat.CreateCard<GreatRamNautilus>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(ram, PileType.Hand, Owner);
        await CardCmd.AutoPlay(choiceContext, ram, target);

        if (shouldTriggerFatal && !target.IsAlive)
            await PowerCmd.Apply<ExplorationPointsPower>(choiceContext, Owner.Creature, FatalExplorePoints,
                Owner.Creature, this);
    }
}
