namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class EntrypointModel
{
    public required string MethodId { get; init; }

    public required string Name { get; init; }

    public required string DeclaringType { get; init; }

    public int ReachableMethods { get; set; }

    public int MaxDepth { get; set; }
}