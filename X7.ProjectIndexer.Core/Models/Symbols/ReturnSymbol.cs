namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class ReturnSymbol
{
    public required string Expression { get; init; }

    public int Line { get; init; }

    public ISymbol? Symbol { get; set; }

    public TypeSymbol? TypeSymbol { get; set; }
}