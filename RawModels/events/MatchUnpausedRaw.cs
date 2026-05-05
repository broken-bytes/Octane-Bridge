using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class MatchUnpausedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
