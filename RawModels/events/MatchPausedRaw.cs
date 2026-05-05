using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class MatchPausedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
