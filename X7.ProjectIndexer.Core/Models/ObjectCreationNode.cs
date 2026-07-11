namespace X7.ProjectIndexer.Core.Models;

public sealed class ObjectCreationNode
{
    public required string Type { get; init; }

    public required string Expression { get; init; }

    public int Line { get; init; }
}