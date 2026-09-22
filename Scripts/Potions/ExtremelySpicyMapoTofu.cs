using Fgo.Scripts.Cards.Colorless.OptionCards;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Potions;

/// <summary>
///     特辣麻婆豆腐：战斗内二选一 —— 「万华镜」（宝具值）或「黑之圣杯」（宝具威力）。
/// </summary>
/// <remarks>
///     <para>
///         【为什么不触发两张卡的 OnPlay】
///         两张卡都是 <c>CardType.Skill</c>。想让 OnPlay 真的跑起来，在禁用
///         <c>CardCmd.AutoPlay</c> 的前提下只剩「手调 <c>CardModel.OnPlayWrapper(..., isAutoPlay: true, ...)</c>」
///         或「手搓 7 个必填字段的 <c>CardPlay</c> 再调 protected 的 OnPlay」两条路 —— 二者都是在手工复刻
///         AutoPlay 的内核（<c>CardCmd.AutoPlay</c> 的最后一步就是调 <c>OnPlayWrapper</c>）却没有它的护栏，会带来：
///         <list type="number">
///             <item>
///                 按 <c>GetResultLocationForCardPlay()</c>，非 Power 卡的结果堆是 <c>PileType.Discard</c>，
///                 两张 Skill 卡会真的落进弃牌堆并被洗回牌库，永久污染牌组。
///             </item>
///             <item>
///                 <c>OnPlayWrapper</c> 会走 <c>Hook.BeforeCardPlayed</c> / <c>Hook.AfterCardPlayed</c>
///                 （autoPlay 还多一个 <c>BeforeCardAutoPlayed</c>），等于"喝一瓶药水 = 打出了两张牌"，
///                 所有"当你打出卡牌时"的 Power / 遗物 / 卡牌都会被误触发。
///             </item>
///             <item><c>GeneratePlayCount</c> 会把 Duplication 类效果带进来，可能把这张伪卡播两次。</item>
///         </list>
///         因此改为：卡只作为选择界面的载体，选完直接调用卡自己暴露的 <c>ResolveEffect</c>。
///         效果仍然只有一处定义（见 Kaleidoscope / TheBlackGrail 的 ResolveEffect），
///         既避开了真实出牌流程，又不会让数值在两处漂移。
///     </para>
///     <para>
///         【联机为什么不需要托管动作】
///         <c>OnUse</c> 由 <c>UsePotionAction</c> 驱动，而该 GameAction 在所有 peer 上各执行一遍，
///         触发本身已被官方动作队列复制，因此不需要 <c>RitsuLibManagedNetActions</c>。
///         选牌经 <c>CardSelectCmd.FromChooseACardScreen</c>，结果按 index 跨端同步，
///         而两个候选都由 <c>combatState.CreateCard</c> 从 canonical 生成 —— 数量与顺序各端完全一致，
///         于是各端用同一个 index 解析到同一张卡。两个效果分别走 <c>FgoResCmd</c> / <c>PowerCmd</c>，
///         各端确定性一致。
///     </para>
/// </remarks>
public class ExtremelySpicyMapoTofu : FgoPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpDamagePower>(),
        FgoHoverTipFactory.FromNp()
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        // 候选从 canonical 生成，保证各端数量与顺序一致（选牌结果按 index 同步）。
        var options = new List<CardModel>
        {
            combatState.CreateCard<Kaleidoscope>(Owner),
            combatState.CreateCard<TheBlackGrail>(Owner)
        };

        var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
        if (selected == null) return;

        switch (selected)
        {
            case Kaleidoscope:
                await FgoResCmd.ModifyNp(100, Owner);
                break;
            case TheBlackGrail:
                await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
                    80, Owner.Creature, null);
                break;
        }
    }
}