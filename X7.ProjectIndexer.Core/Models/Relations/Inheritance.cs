using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class Inheritance
{
    public required TypeSymbol Child { get; init; }

    public required TypeSymbol Parent { get; init; }
}