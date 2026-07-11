namespace X7.ProjectIndexer.Knowledge.ExportModels;

public sealed class SemanticExport
{
    public List<TypeExport> Types { get; } = [];

    public List<MethodExport> Methods { get; } = [];

    public List<DependencyExport> Dependencies { get; } = [];
}