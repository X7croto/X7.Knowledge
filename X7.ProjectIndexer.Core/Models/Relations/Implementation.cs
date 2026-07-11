using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class Implementation
{
    public required TypeSymbol Type { get; init; }

    public required TypeSymbol Interface { get; init; }
}