namespace OctaneBridge.Models;

public sealed class Player
{
    public string Name { get; init; } = "";
    public string PrimaryId { get; init; } = "";
    public int SpectatorId { get; init; }
    public int Team { get; init; }
    public int Score { get; init; }
    public int Goals { get; init; }
    public int Shots { get; init; }
    public int Assists { get; init; }
    public int Saves { get; init; }
    public int Touches { get; init; }
    public int Bumps { get; init; }
    public int Demos { get; init; }
    public bool? HasCar { get; init; }
    public double? Speed { get; init; }
    public int? Boost { get; init; }
    public bool? IsBoosting { get; init; }
    public bool? IsOnGround { get; init; }
    public bool? IsOnWall { get; init; }
    public bool? IsPowersliding { get; init; }
    public bool? IsDemolished { get; init; }
    public bool? IsSupersonic { get; init; }
    public PlayerSummary? Attacker { get; init; }
}

public sealed class Team
{
    public string Name { get; init; } = "";
    public int Id { get; init; }
    public int Score { get; init; }
    public string ColourPrimary { get; init; } = "";
    public string ColourSecondary { get; init; } = "";
}

public sealed class Game
{
    public Team[] Teams { get; init; } = Array.Empty<Team>();
    public int TimeSeconds { get; init; }
    public bool IsOvertime { get; init; }
    public int? Frame { get; init; }
    public double? Elapsed { get; init; }
    public Ball Ball { get; init; } = new();
    public bool IsReplay { get; init; }
    public bool HasWinner { get; init; }
    public string Winner { get; init; } = "";
    public string Arena { get; init; } = "";
    public bool HasTarget { get; init; }
    public PlayerSummary? Target { get; init; }
}

public sealed class UpdateState
{
    public string MatchId { get; init; } = "";
    public Player[] Players { get; init; } = Array.Empty<Player>();
    public Game Game { get; init; } = new();
}
