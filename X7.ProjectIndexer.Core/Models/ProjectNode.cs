namespace X7.ProjectIndexer.Core.Models;

public sealed class ProjectNode
{
    public required string Name { get; init; }

    public required string ProjectFile { get; init; }

    public List<SourceFile> Files { get; } = [];
}