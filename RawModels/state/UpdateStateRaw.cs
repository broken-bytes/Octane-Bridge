using System.Text.Json.Serialization;
using OctaneBridge.RawModels.Events;
using OctaneBridge.RawModels.Models;

namespace OctaneBridge.RawModels.State;

public sealed class PlayerStateRaw
{
    public string Name { get; set; } = "";
    public string PrimaryId { get; set; } = "";
    public int Shortcut { get; set; }
    public int TeamNum { get; set; }
    public int Score { get; set; }
    public int Goals { get; set; }
    public int Shots { get; set; }
    public int Assists { get; set; }
    public int Saves { get; set; }
    public int Touches { get; set; }
    public int CarTouches { get; set; }
    public int Demos { get; set; }

    [JsonPropertyName("bHasCar")] public bool? BHasCar { get; set; }
    public double? Speed { get; set; }
    public int? Boost { get; set; }
    [JsonPropertyName("bBoosting")] public bool? BBoosting { get; set; }
    [JsonPropertyName("bOnGround")] public bool? BOnGround { get; set; }
    [JsonPropertyName("bOnWall")] public bool? BOnWall { get; set; }
    [JsonPropertyName("bPowersliding")] public bool? BPowersliding { get; set; }
    [JsonPropertyName("bDemolished")] public bool? BDemolished { get; set; }
    [JsonPropertyName("bSupersonic")] public bool? BSupersonic { get; set; }

    public PlayerSummaryRaw? Attacker { get; set; }
}

public sealed class TeamRaw
{
    public string Name { get; set; } = "";
    public int TeamNum { get; set; }
    public int Score { get; set; }
    public string ColorPrimary { get; set; } = "";
    public string ColorSecondary { get; set; } = "";
}

public sealed class GameBallRaw
{
    public double Speed { get; set; }
    public int TeamNum { get; set; }
}

public sealed class GameRaw
{
    public TeamRaw[] Teams { get; set; } = Array.Empty<TeamRaw>();
    public int TimeSeconds { get; set; }
    [JsonPropertyName("bOvertime")] public bool BOvertime { get; set; }
    public GameBallRaw Ball { get; set; } = new();
    [JsonPropertyName("bReplay")] public bool BReplay { get; set; }
    [JsonPropertyName("bHasWinner")] public bool BHasWinner { get; set; }
    public string Winner { get; set; } = "";
    public string Arena { get; set; } = "";
    [JsonPropertyName("bHasTarget")] public bool BHasTarget { get; set; }
    public PlayerSummaryRaw? Target { get; set; }
    public int? Frame { get; set; }
    public double? Elapsed { get; set; }
}

public sealed class UpdateStateData : RawData
{
    public PlayerStateRaw[] Players { get; set; } = Array.Empty<PlayerStateRaw>();
    public GameRaw Game { get; set; } = new();
}

public sealed class UpdateStateRaw : RawMessage
{
    public UpdateStateData Data { get; set; } = new();
}
