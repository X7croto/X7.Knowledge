using X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class ArchitectureModel
{
    public List<ModuleModel> Modules { get; } = [];

    public List<ServiceModel> Services { get; } = [];

    public List<EntrypointModel> Entrypoints { get; } = [];

    public List<string> Patterns { get; } = [];

    public List<FlowModel> Flows { get; } = [];

    public List<FeatureModel> Features { get; } = [];

    public List<ConceptModel> Concepts { get; } = [];
}