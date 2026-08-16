using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Powers;

public class NpCardPower : FgoPowerModel
{
    private NobleCardModel? _nobleCard;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     此 power 添加到宝具选择列表的宝具卡（canonical singleton 实例）。
    ///     调用方通过 <c>ModelDb.Card&lt;T&gt;()</c> 泛型方法赋值，无需反射。
    ///     设置时同步更新 <see cref="StringVar" /> 的显示文本，
    ///     使 smartDescription 中的 {NobleCardName} 占位符能正确显示卡牌名称。
    /// </summary>
    public NobleCardModel? NobleCard
    {
        get => _nobleCard;
        set
        {
            _nobleCard = value;
            if (value != null && DynamicVars.TryGetValue("NobleCardName", out var v) && v is StringVar sv)
                sv.StringValue = value.Title;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.String("NobleCardName")
    ];
}