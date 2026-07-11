namespace X7.ProjectIndexer.Core.Models.Graph;

public sealed class SemanticGraph
{
    public List<GraphNode> Nodes { get; } = [];

    public List<GraphEdge> Edges { get; } = [];
}