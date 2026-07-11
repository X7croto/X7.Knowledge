namespace X7.ProjectIndexer.Core.Models.Symbols;

public abstract class BaseSymbol : ISymbol
{
    public required string Id { get; init; }

    public required string Name { get; init; }
}