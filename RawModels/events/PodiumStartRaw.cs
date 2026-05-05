using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class PodiumStartRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
