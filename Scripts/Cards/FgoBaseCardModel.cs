using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
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
    protected virtual bool IgnoreInvincible => false;
    
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

    protected async Task<int> IgnoreInvincibleAction(CardPlay play)
    {
        if (!IgnoreInvincible) return 0;
        if (play.Target is null) return 0;

        var amount = 0;
        if (play.Target.Block != 0)
        {
            play.Target.LoseBlockInternal(play.Target.Block);
            amount++;
        }
        
        if (play.Target.HasPower<HardenedShellPower>())
        {
            await PowerCmd.Remove<HardenedShellPower>(play.Target);
            amount++;
        }

        if (play.Target.HasPower<HardToKillPower>())
        {
            await PowerCmd.Remove<HardToKillPower>(play.Target);
            amount++;
        }
        return amount;
    }
}