public sealed class KnowledgeModel
{
    public ArchitectureModel Architecture { get; } = new();

    public RuntimeModel Runtime { get; } = new();

    public QualityModel Quality { get; } = new();

    public DomainModel Domain { get; } = new();
}