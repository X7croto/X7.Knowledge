using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class Composition
{
    public required TypeSymbol Owner { get; init; }

    public required TypeSymbol Part { get; init; }
}