using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class RoundStartedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
