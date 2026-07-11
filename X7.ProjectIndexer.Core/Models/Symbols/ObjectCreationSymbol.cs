namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class ObjectCreationSymbol
{
    public required string Type { get; init; }

    public required string Expression { get; init; }

    public int Line { get; init; }

    public TypeSymbol? TypeSymbol { get; set; }
}