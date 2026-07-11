using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class ProjectSymbol : ISymbol
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Path { get; init; }

    public List<TypeSymbol> Types { get; } = [];
}