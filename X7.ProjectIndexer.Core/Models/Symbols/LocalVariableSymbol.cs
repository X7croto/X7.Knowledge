namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class LocalVariableSymbol : ISymbol
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public TypeSymbol? TypeSymbol { get; set; }

    public string? TypeQualifiedName { get; set; }
}