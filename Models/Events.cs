namespace OctaneBridge.Models;

public enum EventType
{
    BallHit = 0,
    ClockUpdatedSeconds = 1,
    CountdownBegin = 2,
    CrossbarHit = 3,
    GoalReplayEnd = 4,
    GoalReplayStart = 5,
    GoalReplayWillEnd = 6,
    GoalScored = 7,
    MatchCreated = 8,
    MatchInitialized = 9,
    MatchDestroyed = 10,
    MatchEnded = 11,
    MatchPaused = 12,
    MatchUnpaused = 13,
    PodiumStart = 14,
    ReplayCreated = 15,
    RoundStarted = 16,
    StatfeedEvent = 17
}

public enum StatFeedEventType
{
    Demolish,
    Shot,
    Goal,
    LongGoal,
    HatTrick,
    Save,
    EpicSave,
    Savior,
    Assist,
    Playmaker,
}

public class Event
{
    public EventType Type { get; init; }
    public string MatchId { get; init; } = "";
}

public sealed class Vector3
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public sealed class PlayerSummary
{
    public string Name { get; init; } = "";
    public int SpectatorId { get; init; }
    public int Team { get; init; }
}

public sealed class BallTouch
{
    public PlayerSummary Player { get; init; } = new();
    public double Speed { get; init; }
}

public sealed class Ball
{
    public Vector3? Location { get; init; }
    public double? Speed { get; init; }
    public double? PreHitSpeed { get; init; }
    public double? PostHitSpeed { get; init; }
    public int? TeamId { get; init; }
}

public sealed class BallHitEvent : Event
{
    public PlayerSummary[] Players { get; init; } = Array.Empty<PlayerSummary>();
    public Ball Ball { get; init; } = new();
}

public sealed class ClockUpdatedEvent : Event
{
    public double TimeInSeconds { get; init; }
    public bool IsOvertime { get; init; }
}

public sealed class CrossbarHitEvent : Event
{
    public Ball Ball { get; init; } = new();
    public double ImpactForce { get; init; }
    public BallTouch LastTouch { get; init; } = new();
}

public sealed class GoalScoredEvent : Event
{
    public double GoalSpeed { get; init; }
    public double GoalTime { get; init; }
    public Vector3 ImpactLocation { get; init; } = new();
    public PlayerSummary Scorer { get; init; } = new();
    public PlayerSummary? Assister { get; init; }
    public BallTouch LastTouch { get; init; } = new();
}

public sealed class MatchEndedEvent : Event
{
    public int WinnerTeamId { get; init; }
}

public sealed class StatFeedEvent : Event
{
    public StatFeedEventType Stat { get; init; }
    public PlayerSummary MainTarget { get; init; } = new();
    public PlayerSummary? SecondaryTarget { get; init; }
}
