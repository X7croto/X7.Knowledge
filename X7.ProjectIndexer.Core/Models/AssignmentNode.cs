namespace X7.ProjectIndexer.Core.Models;

public sealed class AssignmentNode
{
    public required string Left { get; init; }

    public required string Right { get; init; }

    public int Line { get; init; }
}