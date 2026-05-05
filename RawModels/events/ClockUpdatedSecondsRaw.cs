using System.Text.Json.Serialization;
using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class ClockUpdatedSecondsData : RawData
{
    public double TimeSeconds { get; set; }

    [JsonPropertyName("bOvertime")]
    public bool BOvertime { get; set; }
}

public sealed class ClockUpdatedSecondsRaw : RawMessage
{
    public ClockUpdatedSecondsData Data { get; set; } = new();
}
