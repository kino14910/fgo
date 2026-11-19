using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.DerivativeNemo;

/// <summary>
///     上浮吧鹦鹉螺号: 对一名敌人打出<b>鹦鹉螺的大冲角</b>；若斩杀该敌人，获得 2 点探索点数。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
/// <remarks>斩杀判定与官方 Feed 同源（先过「所有能力允许死亡触发斩杀」门槛，再确认目标死了）。AutoPlay 拿不到 AttackCommand.Results，故用「攻击前存活→攻击后不存活」等价判定：IsAlive = CurrentHp>0，被毅力救回仍算存活，比 WasTargetKilled 的血量触底更贴合「斩杀」。</remarks>
public class ToTheSurfaceNautilus() : FgoBaseCardModel(0, CardType.Power,
    CardRarity.Token, TargetType.AnyEnemy,
    false)
{
    private const decimal FatalExplorePoints = 2m;

    public override bool CanBeGeneratedInCombat => false;

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

        var ram = combat.CreateCard<GreatRamNautilus>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(ram, PileType.Hand, Owner);
        await CardCmd.AutoPlay(choiceContext, ram, target);

        if (shouldTriggerFatal && !target.IsAlive)
            await PowerCmd.Apply<ExplorationPointsPower>(choiceContext, Owner.Creature, FatalExplorePoints,
                Owner.Creature, this);
    }
}