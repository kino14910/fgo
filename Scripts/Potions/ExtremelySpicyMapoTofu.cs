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