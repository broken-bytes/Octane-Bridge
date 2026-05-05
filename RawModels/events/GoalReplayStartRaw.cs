using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class GoalReplayStartRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
