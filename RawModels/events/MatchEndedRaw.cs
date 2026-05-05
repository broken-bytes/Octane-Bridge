using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.Events;

public sealed class MatchEndedData : RawData
{
    public int WinnerTeamNum { get; set; }
}

public sealed class MatchEndedRaw : RawMessage
{
    public MatchEndedData Data { get; set; } = new();
}
