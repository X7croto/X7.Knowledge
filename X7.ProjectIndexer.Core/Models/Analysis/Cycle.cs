using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class Cycle
{
    public List<MethodSymbol> Methods { get; } = [];
}