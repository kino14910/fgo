using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace Fgo.Scripts.Powers;

public class GodsExecutionPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<GodsExecution>();

    protected override bool IsPositive => false;

    public PowerAssetProfile AssetProfile => new($"res://Fgo/images/powers/AtkDownPower.png",
        $"res://Fgo/images/powers/AtkDownPower.png");

    public virtual string? CustomIconPath => AssetProfile.IconPath;
    public virtual string? CustomBigIconPath => AssetProfile.BigIconPath;

}