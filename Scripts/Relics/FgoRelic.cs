using Fgo.Scripts.Character;
using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Relics;

[RegisterRelic(typeof(FgoRelicPool), Inherit = true)]
// [RegisterCharacterStarterRelic(typeof(FgoCharacter))] // 注册起始遗物
public abstract class FgoRelic : ModRelicTemplate
{
    /// <summary>
    ///     圣晶石/召唤券共享计数: 存于按玩家的 FgoRunState 而非遗物实例。
    ///     点金石精炼（SaintQuartz → SummonTicket）通过 RelicCmd.Replace 替换遗物实例，
    ///     挂在实例上的状态（SavedAttachedState）会丢失，按玩家数据则不受影响，计数得以保留。
    ///     Owner 为 null 时（卡牌图鉴/遗物收藏预览的 canonical 单例）返回 0。
    /// </summary>
    protected int QuartzCounter
    {
        get => Owner != null ? Entry.RunState.Get(Owner).QuartzCount : 0;
        set
        {
            if (Owner != null)
                Entry.RunState.Modify(Owner, data => data.QuartzCount = value);
        }
    }

    public override RelicAssetProfile AssetProfile
    {
        get
        {
            // 有专属图标用专属图标，否则回退到占位 relic.png
            // （与 FgoPowerModel 的 fallback 策略一致，避免新遗物缺图时显示异常）。
            var small = $"res://Fgo/images/relics/{GetType().Name}.png";
            var big = $"res://Fgo/images/relics/big/{GetType().Name}.png";
            var smallExists = ResourceLoader.Exists(small);
            return new RelicAssetProfile(
                smallExists ? small : "res://Fgo/images/relics/relic.png",
                smallExists ? small : "res://Fgo/images/relics/relic.png",
                ResourceLoader.Exists(big) ? big : "res://Fgo/images/relics/big/relic.png"
            );
        }
    }

    /// <summary>
    ///     当计数达到可进行一次宝具抽取的阈值时，把遗物状态置为 Active（图标高亮发光），
    ///     提醒玩家可以右键圣晶石/召唤券进行宝具抽取；计数不足时恢复 Normal。
    ///     参照原版 PollinousCore 用 RelicStatus.Active 反映"可激活"状态的做法。
    /// </summary>
    protected void UpdateAvailableVisual(int threshold)
    {
        Status = QuartzCounter >= threshold ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    ///     供模块外部（事件消耗圣晶石、战斗内好感度变化）刷新遗物的可激活高亮与计数显示。
    /// </summary>
    public void RefreshCounterVisual(int threshold)
    {
        UpdateAvailableVisual(threshold);
    }

    public void InvokeDisplayAmountChanged()
    {
        base.InvokeDisplayAmountChanged();
    }
}