namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class Reference
{
    public required string Source { get; init; }

    public required string Target { get; init; }

    public required string Kind { get; init; }

    public string? File { get; init; }

    public int Line { get; init; }
}