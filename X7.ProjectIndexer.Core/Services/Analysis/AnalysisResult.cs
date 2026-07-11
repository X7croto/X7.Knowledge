namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class AnalysisResult
{
    public Metrics Metrics { get; } = new();

    public List<DeadMethod> DeadMethods { get; } = [];

    public List<ImpactedMethod> ImpactedMethods { get; } = [];

    public List<Cycle> Cycles { get; } = [];

    public List<TypeMetrics> TypeMetrics { get; } = [];

    public List<Layer> Layers { get; } = [];

    public List<ArchitecturalViolation> Violations { get; } = [];
}