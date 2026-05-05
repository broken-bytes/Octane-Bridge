using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class GoalReplayEndRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
