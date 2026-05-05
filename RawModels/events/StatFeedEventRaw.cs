using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class StatFeedEventData : RawData
{
    public string EventName { get; set; } = "";
    public string Type { get; set; } = "";
    public PlayerSummaryRaw MainTarget { get; set; } = new();
    public PlayerSummaryRaw? SecondaryTarget { get; set; }
}

public sealed class StatFeedEventRaw : RawMessage
{
    public StatFeedEventData Data { get; set; } = new();
}
