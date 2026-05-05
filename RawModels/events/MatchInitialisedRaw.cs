using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class MatchInitialisedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
