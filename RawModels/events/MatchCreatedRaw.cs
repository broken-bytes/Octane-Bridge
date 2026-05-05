using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class MatchCreatedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
