using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class CrossbarHitData : RawData
{
    public Vector3Raw BallLocation { get; set; } = new();
    public double BallSpeed { get; set; }
    public double ImpactForce { get; set; }
    public BallTouchRaw BallLastTouch { get; set; } = new();
}

public sealed class CrossbarHitRaw : RawMessage
{
    public CrossbarHitData Data { get; set; } = new();
}
