using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class GoalScoredData : RawData
{
    public double GoalSpeed { get; set; }
    public double GoalTime { get; set; }
    public Vector3Raw ImpactLocation { get; set; } = new();
    public PlayerSummaryRaw Scorer { get; set; } = new();
    public PlayerSummaryRaw? Assister { get; set; }
    public BallTouchRaw BallLastTouch { get; set; } = new();
}

public sealed class GoalScoredRaw : RawMessage
{
    public GoalScoredData Data { get; set; } = new();
}
