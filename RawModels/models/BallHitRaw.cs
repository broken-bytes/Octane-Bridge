using OctaneBridge.RawModels.Events;

namespace OctaneBridge.RawModels.Models;

public sealed class BallHitBallRaw
{
    public double PreHitSpeed { get; set; }
    public double PostHitSpeed { get; set; }
    public Vector3Raw Location { get; set; } = new();
}

public sealed class BallHitData : RawData
{
    public PlayerSummaryRaw[] Players { get; set; } = Array.Empty<PlayerSummaryRaw>();
    public BallHitBallRaw Ball { get; set; } = new();
}

public sealed class BallHitRaw : RawMessage
{
    public BallHitData Data { get; set; } = new();
}
