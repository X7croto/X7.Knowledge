namespace X7.ProjectIndexer.Core.Models;

public sealed class IfNode
{
    public required string Condition { get; init; }

    public int Line { get; init; }
}