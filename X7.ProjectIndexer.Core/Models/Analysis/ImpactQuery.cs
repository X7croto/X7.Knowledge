using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class ImpactQuery
{
    public required MethodSymbol Root { get; init; }

    public List<MethodSymbol> AffectedMethods { get; } = [];

    public List<TypeSymbol> AffectedTypes { get; } = [];

    public ImpactLevel Level { get; set; }

    public int MethodCount => AffectedMethods.Count;

    public int TypeCount => AffectedTypes.Count;
}