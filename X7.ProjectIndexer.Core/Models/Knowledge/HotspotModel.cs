namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class HotspotModel
{
    public required string TypeId { get; init; }

    public required string Name { get; init; }

    public required string Namespace { get; init; }

    public int Score { get; init; }

    public double Instability { get; init; }

    public int FanIn { get; init; }

    public int FanOut { get; init; }
}