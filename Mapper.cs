using OctaneBridge.Models;
using OctaneBridge.RawModels.Events;
using OctaneBridge.RawModels.Models;
using OctaneBridge.RawModels.State;

namespace OctaneBridge;

public sealed record MappedMessage(string Channel, object Payload);

public static class Mapper
{
    public const string EventsChannel = "/events";
    public const string StateChannel = "/state";
    public const string MetaChannel = "/meta";

    public static MappedMessage? Map(RawMessage raw)
    {
        if (raw is UpdateStateRaw u)
        {
            return new MappedMessage(StateChannel, MapUpdateState(u.Data));
        }

        var evt = MapEvent(raw);
        return evt is null ? null : new MappedMessage(EventsChannel, evt);
    }

    private static Event? MapEvent(RawMessage raw) => raw switch
    {
        BallHitRaw r => new BallHitEvent
        {
            Type = EventType.BallHit,
            MatchId = r.Data.MatchGuid,
            Players = r.Data.Players.Select(MapPlayerSummary).ToArray(),
            Ball = new Ball
            {
                Location = MapVector(r.Data.Ball.Location),
                PreHitSpeed = r.Data.Ball.PreHitSpeed,
                PostHitSpeed = r.Data.Ball.PostHitSpeed
            }
        },
        ClockUpdatedSecondsRaw r => new ClockUpdatedEvent
        {
            Type = EventType.ClockUpdatedSeconds,
            MatchId = r.Data.MatchGuid,
            TimeInSeconds = r.Data.TimeSeconds,
            IsOvertime = r.Data.BOvertime
        },
        CountdownBeginRaw r => new Event { Type = EventType.CountdownBegin, MatchId = r.Data.MatchGuid },
        CrossbarHitRaw r => new CrossbarHitEvent
        {
            Type = EventType.CrossbarHit,
            MatchId = r.Data.MatchGuid,
            Ball = new Ball { Location = MapVector(r.Data.BallLocation), Speed = r.Data.BallSpeed },
            ImpactForce = r.Data.ImpactForce,
            LastTouch = MapBallTouch(r.Data.BallLastTouch)
        },
        GoalReplayEndRaw r => new Event { Type = EventType.GoalReplayEnd, MatchId = r.Data.MatchGuid },
        GoalReplayStartRaw r => new Event { Type = EventType.GoalReplayStart, MatchId = r.Data.MatchGuid },
        GoalReplayWillEndRaw r => new Event { Type = EventType.GoalReplayWillEnd, MatchId = r.Data.MatchGuid },
        GoalScoredRaw r => new GoalScoredEvent
        {
            Type = EventType.GoalScored,
            MatchId = r.Data.MatchGuid,
            GoalSpeed = r.Data.GoalSpeed,
            GoalTime = r.Data.GoalTime,
            ImpactLocation = MapVector(r.Data.ImpactLocation),
            Scorer = MapPlayerSummary(r.Data.Scorer),
            Assister = r.Data.Assister is null ? null : MapPlayerSummary(r.Data.Assister),
            LastTouch = MapBallTouch(r.Data.BallLastTouch)
        },
        MatchCreatedRaw r => new Event { Type = EventType.MatchCreated, MatchId = r.Data.MatchGuid },
        MatchInitialisedRaw r => new Event { Type = EventType.MatchInitialized, MatchId = r.Data.MatchGuid },
        MatchDestroyedRaw r => new Event { Type = EventType.MatchDestroyed, MatchId = r.Data.MatchGuid },
        MatchEndedRaw r => new MatchEndedEvent
        {
            Type = EventType.MatchEnded,
            MatchId = r.Data.MatchGuid,
            WinnerTeamId = r.Data.WinnerTeamNum
        },
        MatchPausedRaw r => new Event { Type = EventType.MatchPaused, MatchId = r.Data.MatchGuid },
        MatchUnpausedRaw r => new Event { Type = EventType.MatchUnpaused, MatchId = r.Data.MatchGuid },
        PodiumStartRaw r => new Event { Type = EventType.PodiumStart, MatchId = r.Data.MatchGuid },
        ReplayCreatedRaw r => new Event { Type = EventType.ReplayCreated, MatchId = r.Data.MatchGuid },
        RoundStartedRaw r => new Event { Type = EventType.RoundStarted, MatchId = r.Data.MatchGuid },
        StatFeedEventRaw r => MapStatFeed(r),
        _ => null
    };

    private static Event? MapStatFeed(StatFeedEventRaw r)
    {
        var stat = r.Data.EventName switch
        {
            "Demolish" => (StatFeedEventType?)StatFeedEventType.Demolish,
            "Shot" => StatFeedEventType.Shot,
            "Goal" => StatFeedEventType.Goal,
            "LongGoal" => StatFeedEventType.LongGoal,
            "HatTrick" => StatFeedEventType.HatTrick,
            "Save" => StatFeedEventType.Save,
            "EpicSave" => StatFeedEventType.EpicSave,
            "Savior" => StatFeedEventType.Savior,
            "Assist" => StatFeedEventType.Assist,
            "Playmaker" => StatFeedEventType.Playmaker,
            _ => null
        };
        if (stat is null) return null;
        return new StatFeedEvent
        {
            Type = EventType.StatfeedEvent,
            MatchId = r.Data.MatchGuid,
            Stat = stat.Value,
            MainTarget = MapPlayerSummary(r.Data.MainTarget),
            SecondaryTarget = r.Data.SecondaryTarget is null ? null : MapPlayerSummary(r.Data.SecondaryTarget)
        };
    }

    private static UpdateState MapUpdateState(UpdateStateData d) => new()
    {
        MatchId = d.MatchGuid,
        Players = d.Players.Select(MapPlayer).ToArray(),
        Game = MapGame(d.Game)
    };

    private static Player MapPlayer(PlayerStateRaw r) => new()
    {
        Name = r.Name,
        PrimaryId = r.PrimaryId,
        SpectatorId = r.Shortcut,
        Team = r.TeamNum,
        Score = r.Score,
        Goals = r.Goals,
        Shots = r.Shots,
        Assists = r.Assists,
        Saves = r.Saves,
        Touches = r.Touches,
        Bumps = r.CarTouches,
        Demos = r.Demos,
        HasCar = r.BHasCar,
        Speed = r.Speed,
        Boost = r.Boost,
        IsBoosting = r.BBoosting,
        IsOnGround = r.BOnGround,
        IsOnWall = r.BOnWall,
        IsPowersliding = r.BPowersliding,
        IsDemolished = r.BDemolished,
        IsSupersonic = r.BSupersonic,
        Attacker = r.Attacker is null ? null : MapPlayerSummary(r.Attacker)
    };

    private static Team MapTeam(TeamRaw r) => new()
    {
        Name = r.Name,
        Id = r.TeamNum,
        Score = r.Score,
        ColourPrimary = r.ColorPrimary,
        ColourSecondary = r.ColorSecondary
    };

    private static Game MapGame(GameRaw r) => new()
    {
        Teams = r.Teams.Select(MapTeam).ToArray(),
        TimeSeconds = r.TimeSeconds,
        IsOvertime = r.BOvertime,
        Frame = r.Frame,
        Elapsed = r.Elapsed,
        Ball = new Ball { Speed = r.Ball.Speed, TeamId = r.Ball.TeamNum },
        IsReplay = r.BReplay,
        HasWinner = r.BHasWinner,
        Winner = r.Winner,
        Arena = r.Arena,
        HasTarget = r.BHasTarget,
        Target = r.Target is null ? null : MapPlayerSummary(r.Target)
    };

    private static Vector3 MapVector(Vector3Raw r) => new() { X = r.X, Y = r.Y, Z = r.Z };

    private static PlayerSummary MapPlayerSummary(PlayerSummaryRaw r) =>
        new() { Name = r.Name, SpectatorId = r.Shortcut, Team = r.TeamNum };

    private static BallTouch MapBallTouch(BallTouchRaw r) =>
        new() { Player = MapPlayerSummary(r.Player), Speed = r.Speed };
}
