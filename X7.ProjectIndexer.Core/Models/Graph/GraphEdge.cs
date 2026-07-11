namespace X7.ProjectIndexer.Core.Models.Graph;

public sealed class GraphEdge
{
    public required string SourceId { get; init; }

    public required string TargetId { get; init; }

    public required string Relation { get; init; }
}