using Fgo.Scripts.Character;
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

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        $"res://Fgo/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        $"res://Fgo/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        $"res://Fgo/images/relics/{GetType().Name}.png"
    );
}