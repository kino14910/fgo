using Fgo.Scripts.Keywords;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

public abstract class FgoBaseCardModel(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    /// <summary>
    ///     默认 AssetProfile: 根据 CardType 选择卡框；派生类可重写。
    /// </summary>
    public override CardAssetProfile AssetProfile => new(
        $"res://Fgo/images/cards/big/{GetType().Name}.png",
        ResourceLoader.Exists($"res://Fgo/images/cards/big/beta/{GetType().Name}.png")
            ? $"res://Fgo/images/cards/big/beta/{GetType().Name}.png"
            : null,
        Type switch
        {
            CardType.Attack => "res://Fgo/images/card_frames/card_frame_attack.png",
            CardType.Skill => "res://Fgo/images/card_frames/card_frame_skill.png",
            CardType.Power => "res://Fgo/images/card_frames/card_frame_power.png",
            _ => "res://Fgo/images/card_frames/card_frame_skill.png"
        }
    );

    /// <summary>
    ///     无敌贯通效果：仅当卡牌自身带有 [gold]无敌贯通[/gold] 关键词时生效，
    ///     去除目标的格挡、[gold]硬化外壳[/gold] 和 [gold]难以杀灭[/gold]。
    /// </summary>
    protected async Task<int> IgnoreInvincibleAction(Creature target)
    {
        if (!Keywords.Contains(FgoKeywords.IgnoreInvincible)) return 0;
        if (!target.IsMonster) return 0;

        var amount = 0;
        if (target.Block != 0)
        {
            target.LoseBlockInternal(target.Block);
            amount++;
        }

        if (target.HasPower<HardenedShellPower>())
        {
            await PowerCmd.Remove<HardenedShellPower>(target);
            amount++;
        }

        if (target.HasPower<HardToKillPower>())
        {
            await PowerCmd.Remove<HardToKillPower>(target);
            amount++;
        }

        return amount;
    }

    protected async Task<int> IgnoreInvincibleAction(IEnumerable<Creature>? targets)
    {
        if (targets == null) return 0;
        var amount = 0;
        foreach (var target in targets) amount += await IgnoreInvincibleAction(target);

        return amount;
    }
}