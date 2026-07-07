using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Events;

public abstract class FgoEventModel : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"res://images/events/{GetType().Name}.png"
    );
}