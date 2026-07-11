namespace X7.ProjectIndexer.Core.Models.Graph;

public sealed class GraphNode
{
    public required string Id { get; init; }

    public required string Kind { get; init; }

    public object Symbol { get; init; } = default!;
}