using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class Dependency2
{
    public required TypeSymbol Source { get; init; }

    public required TypeSymbol Target { get; init; }

    public required DependencyKind Kind { get; init; }
}