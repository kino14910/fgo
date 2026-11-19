using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.UI;

/// <remarks>场景根必须挂 NCombatBackground 脚本（本类即其派生），否则 Instantiate&lt;NCombatBackground&gt; 抛 InvalidCastException。</remarks>
public sealed partial class FgoVoidSeaBackgroundRoot : NCombatBackground
{
}