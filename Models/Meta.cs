namespace OctaneBridge.Models;

public sealed class OctaneMetaSide
{
    public string Name { get; init; } = "";
    public string Logo { get; init; } = "";
    public int Wins { get; init; }
}

public sealed class OctaneMeta
{
    public int BestOf { get; init; } = 5;
    public OctaneMetaSide Blue { get; init; } = new();
    public OctaneMetaSide Orange { get; init; } = new();
}
