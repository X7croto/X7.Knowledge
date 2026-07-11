namespace X7.ProjectIndexer.Knowledge.ExportModels;

public sealed class DependencyExport
{
    public required string SourceId { get; init; }

    public required string TargetId { get; init; }
}