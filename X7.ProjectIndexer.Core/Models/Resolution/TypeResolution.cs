using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Resolution;

public sealed class TypeResolution
{
    public string Name { get; init; } = "";

    public TypeSymbol? Symbol { get; init; }

    public TypeOrigin Origin { get; init; }

    public bool Resolved => Symbol is not null;
}