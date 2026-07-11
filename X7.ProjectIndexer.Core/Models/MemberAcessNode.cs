namespace X7.ProjectIndexer.Core.Models;

public sealed class MemberAccessNode
{
    public required string Expression { get; init; }

    public required string Target { get; init; }

    public required string Member { get; init; }

    public int Line { get; init; }
}