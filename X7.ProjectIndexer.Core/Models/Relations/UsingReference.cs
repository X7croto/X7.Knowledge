namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class UsingReference
{
    public required string File { get; init; }

    public required string Namespace { get; init; }
}