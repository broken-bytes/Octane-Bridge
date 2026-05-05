using OctaneBridge.RawModels.Events;

namespace OctaneBridge.RawModels.Models;

public sealed class BallTouchRaw
{
    public PlayerSummaryRaw Player { get; set; } = new();
    public double Speed { get; set; }
}
