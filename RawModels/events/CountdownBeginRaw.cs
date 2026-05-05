using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class CountdownBeginRaw : RawMessage
{
    public RawData Data { get; set; } = new();
}
