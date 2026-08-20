using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace Fgo.Scripts.Powers;

public class SevendrivePower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<Sevendrive>();

    public PowerAssetProfile AssetProfile => new("res://Fgo/images/powers/AtkUpPower.png",
        "res://Fgo/images/powers/AtkUpPower.png");

    public virtual string? CustomIconPath => AssetProfile.IconPath;
    public virtual string? CustomBigIconPath => AssetProfile.BigIconPath;
}