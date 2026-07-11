namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class ServiceDescription
{
    public ServiceKind Kind { get; init; }

    public int Confidence { get; init; }

    public List<string> Reasons { get; init; } = [];
}