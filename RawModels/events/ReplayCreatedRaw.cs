using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class ReplayCreatedRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
