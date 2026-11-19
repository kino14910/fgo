using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.UI;

/// <remarks>
///     场景根必须挂 <see cref="NCombatBackground" /> 脚本（本类即为其派生）。
///     初始化期校验以 <c>Instantiate&lt;Node&gt;</c> + 类型检查把这个不变量变成显式失败：
///     DEBUG 直接抛出，RELEASE 记录并熔断背景功能。
/// </remarks>
public sealed partial class FgoVoidSeaBackgroundRoot : NCombatBackground
{
}