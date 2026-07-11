using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class Layer
{
    public required int Level { get; init; }

    public List<TypeSymbol> Types { get; } = [];
}