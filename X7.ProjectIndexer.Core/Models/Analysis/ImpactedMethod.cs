using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class ImpactedMethod
{
    public required MethodSymbol Source { get; init; }

    public required MethodSymbol Impacted { get; init; }

    public int Distance { get; init; }
}