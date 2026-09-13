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
        get => Entry.RunState.Get(Owner).QuartzCount;
        set => Entry.RunState.Modify(Owner, data => data.QuartzCount = value);
    }

    public override RelicAssetProfile AssetProfile
    {
        get
        {
            // 有专属图标用专属图标，否则回退到占位 relic.png
            // （与 FgoPowerModel 的 fallback 策略一致，避免新遗物缺图时显示异常）。
            var small = $"res://Fgo/images/relics/{GetType().Name}.png";
            var big = $"res://Fgo/images/relics/big/{GetType().Name}.png";
            var outline = $"res://Fgo/images/relics/outline/{GetType().Name}.png";
            return new RelicAssetProfile(
                ResourceLoader.Exists(small) ? small : "res://Fgo/images/relics/relic.png",
                ResourceLoader.Exists(outline) ? outline : "res://Fgo/images/relics/outline/relic.png",
                ResourceLoader.Exists(big) ? big : "res://Fgo/images/relics/big/relic.png"
            );
        }
    }

    /// <summary>
    ///     根据当前圣晶石计数与阈值刷新遗物的可激活高亮：
    ///     计数达到阈值时把遗物状态置为 Active（图标发光），提醒可右键抽取宝具；不足时恢复 Normal。
    ///     同时触发计数显示刷新。遗物子类与事件（消耗圣晶石后）均调用此方法。
    /// </summary>
    public void RefreshQuartzActivationVisual(int threshold)
    {
        Status = QuartzCounter >= threshold ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    protected new void InvokeDisplayAmountChanged()
    {
        base.InvokeDisplayAmountChanged();
    }

    /// <summary>
    ///     进入房间时 +1 圣晶石计数。各端本地自增（AfterRoomEntered 是被复制的房间动作，正常推进时所有端重放即一致）。
    ///     联机读档重放当前房间导致客机多 +1 的问题由 FgoQuartzSync.ShouldSkipRoomEntry 在进入前拦截，
    ///     主机额外广播权威值兜底，无需在此做幂等。
    /// </summary>
    protected void IncrementQuartzOnRoomEntry(int threshold)
    {
        QuartzCounter++;
        RefreshQuartzActivationVisual(threshold);
    }
}