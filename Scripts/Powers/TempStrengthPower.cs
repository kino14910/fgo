using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

[RegisterPower(Inherit = true)]
public abstract class TempStrengthPower<T> : ModTemporaryAppliedPowerTemplate<T, StrengthPower> where T : AbstractModel
{
    // 自定义图标路径。

    public override PowerAssetProfile AssetProfile => IsPositive
        ? new PowerAssetProfile(
            "res://Fgo/images/powers/AtkUpPower.png",
            "res://Fgo/images/powers/AtkUpPower.png"
        )
        : new PowerAssetProfile(
            "res://Fgo/images/powers/AtkDownPower.png",
            "res://Fgo/images/powers/AtkDownPower.png");

    // protected override bool IsPositive => false; // 正面效果还是负面

    // protected override bool UntilEndOfOtherSideTurn => false; // 为 true 时，在另一方回合结束时过期；否则在拥有者一方回合结束时过期。

    // protected override int LastForXExtraTurns => 0; // 额外持续回合数

    // 推荐重载描述，以达到多个power共享一条文本的效果
    // 例如这里的文本需要在powers.json中写"TEST_POWER_TEMP_POWER.description"和"TEST_POWER_TEMP_POWER_DOWN.description"
    public override LocString Description => new("powers",
        IsPositive ? "TEMPORARY_STRENGTH_POWER.description" : "TEMPORARY_STRENGTH_DOWN.description");
}