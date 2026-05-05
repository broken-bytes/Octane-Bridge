using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class MatchDestroyedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
