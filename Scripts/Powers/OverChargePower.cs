using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     打出任意宝具牌后会获得层数，最多 <see cref="MaxOverCharge" /> 层；
///     选择宝具页面按本 power 的层数升级被选中的宝具副本。
/// </summary>
public class OverchargePower : FgoPowerModel
{
    public const int MaxOverCharge = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/OverchargePower.png",
        "res://Fgo/images/powers/big/OverchargePower.png"
    );
}