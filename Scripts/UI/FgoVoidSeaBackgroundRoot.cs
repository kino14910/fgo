using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.UI;

/// <summary>
///     〔虚数空间〕战斗背景场景的根脚本。
/// </summary>
/// <remarks>
///     <para>
///         <b>这个类不能省。</b><c>NCombatBackground.Create</c>（以及本 mod 的等价写法）都用
///         <c>PackedScene.Instantiate&lt;NCombatBackground&gt;(...)</c> 取背景，而该重载要求
///         <b>场景根节点本身就是 <see cref="NCombatBackground" /> 的实例</b>。
///         场景根若只写 <c>type="Control"</c> 且不挂脚本，就会抛
///         <c>InvalidCastException</c>，且因为外层有 try/catch 兜底，只留一行错误日志、
///         背景永远换不出来——排查时极易误判成"注册没生效"。
///     </para>
///     <para>
///         原版每个背景 .tscn 的根都是挂了 <c>NCombatBackground</c> 脚本的 Control，
///         这里照做：根挂本类，<c>Layer_00</c> 等图层容器仍按约定命名。
///     </para>
/// </remarks>
public sealed partial class FgoVoidSeaBackgroundRoot : NCombatBackground
{
}
