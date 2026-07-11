namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class InvocationNode
{
    public required string Name { get; init; }

    public string? Target { get; set; }

    public required string Expression { get; init; }

    public int Line { get; init; }
}