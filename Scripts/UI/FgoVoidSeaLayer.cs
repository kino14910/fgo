using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.UI;

/// <summary>
///     〔虚数空间〕背景的图层脚本。
/// </summary>
/// <remarks>
///     必须是 <see cref="NCombatBackgroundLayer" /> 的子类: 基类 <c>_Ready()</c> 会
///     <c>GetNode&lt;Control&gt;("Visual")</c> 与 <c>GetNode&lt;Control&gt;("PhobiaModeVisual")</c>，
///     两者都不做 null 容忍，缺一个图层进树就抛异常，并且它还要靠这两个节点响应
///     「恐惧模式」开关（<c>UpdatePhobiaMode</c> 在两者间切换显隐）。
///     场景里挂上本类、并保证这两个直属子节点存在，即可满足契约；
///     没有额外行为，因此这里不再重写任何成员。
/// </remarks>
public sealed partial class FgoVoidSeaLayer : NCombatBackgroundLayer
{
}
