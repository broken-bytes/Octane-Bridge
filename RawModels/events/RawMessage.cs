using System.Text.Json.Serialization;
using OctaneBridge.RawModels.Models;
using OctaneBridge.RawModels.State;

namespace OctaneBridge.RawModels.Events;

[JsonPolymorphic(
    TypeDiscriminatorPropertyName = "Event",
    IgnoreUnrecognizedTypeDiscriminators = true)]
[JsonDerivedType(typeof(BallHitRaw), "BallHit")]
[JsonDerivedType(typeof(UpdateStateRaw), "UpdateState")]
[JsonDerivedType(typeof(ClockUpdatedSecondsRaw), "ClockUpdatedSeconds")]
[JsonDerivedType(typeof(CountdownBeginRaw), "CountdownBegin")]
[JsonDerivedType(typeof(CrossbarHitRaw), "CrossbarHit")]
[JsonDerivedType(typeof(GoalReplayEndRaw), "GoalReplayEnd")]
[JsonDerivedType(typeof(GoalReplayStartRaw), "GoalReplayStart")]
[JsonDerivedType(typeof(GoalReplayWillEndRaw), "GoalReplayWillEnd")]
[JsonDerivedType(typeof(GoalScoredRaw), "GoalScored")]
[JsonDerivedType(typeof(MatchCreatedRaw), "MatchCreated")]
[JsonDerivedType(typeof(MatchInitialisedRaw), "MatchInitialized")]
[JsonDerivedType(typeof(MatchDestroyedRaw), "MatchDestroyed")]
[JsonDerivedType(typeof(MatchEndedRaw), "MatchEnded")]
[JsonDerivedType(typeof(MatchPausedRaw), "MatchPaused")]
[JsonDerivedType(typeof(MatchUnpausedRaw), "MatchUnpaused")]
[JsonDerivedType(typeof(PodiumStartRaw), "PodiumStart")]
[JsonDerivedType(typeof(ReplayCreatedRaw), "ReplayCreated")]
[JsonDerivedType(typeof(RoundStartedRaw), "RoundStarted")]
[JsonDerivedType(typeof(StatFeedEventRaw), "StatfeedEvent")]
public class RawMessage { }
