namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class IdentifierSymbol
{
    public required string Name { get; init; }

    public int Line { get; init; }

    public ISymbol? Symbol { get; set; }
}