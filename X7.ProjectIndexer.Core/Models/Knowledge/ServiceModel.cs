using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class ServiceModel
{
    public required string Name { get; init; }

    public required string Namespace { get; init; }

    public required TypeSymbol Symbol { get; init; }

    public required ServiceDescription Description { get; init; }

    public string Layer { get; set; } = "Unknown";

    public List<string> Dependencies { get; } = [];
}