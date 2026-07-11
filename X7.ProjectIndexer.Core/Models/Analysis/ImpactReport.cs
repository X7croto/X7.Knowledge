using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class ImpactReport
{
    public required MethodSymbol Root { get; init; }

    public required ImpactLevel Level { get; init; }

    public List<TypeSymbol> Types { get; } = [];

    public List<MethodSymbol> Methods { get; } = [];
}