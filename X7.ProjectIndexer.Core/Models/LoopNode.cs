namespace X7.ProjectIndexer.Core.Models;

public sealed class LoopNode
{
    public required string Kind { get; init; }

    public string? Condition { get; init; }

    public int Line { get; init; }
}