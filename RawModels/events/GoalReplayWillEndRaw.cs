using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class GoalReplayWillEndRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
