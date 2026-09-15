using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     打出任意宝具牌后会获得层数，最多 <see cref="MaxOvercharge" /> 层；
///     选择宝具页面按本 power 的层数强化被选中的宝具副本。
/// </summary>
public class OverchargePower : FgoPowerModel
{
    public const int MaxOvercharge = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     层数角标显示封顶。真实层数已在 <c>FgoBattleHooks.TryModifyPowerAmountReceived</c> 里
    ///     于写入前钳进上限（<see cref="MegaCrit.Sts2.Core.Models.PowerModel.Amount" /> 的 getter
    ///     非 virtual，无法重写来约束层数）；这里只兜住旧存档等历史超限状态，避免角标显示出 4 以上的层数。
    /// </summary>
    public override int DisplayAmount => Math.Min(Amount, MaxOvercharge);

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/OverchargePower.png",
        "res://Fgo/images/powers/big/OverchargePower.png"
    );
}