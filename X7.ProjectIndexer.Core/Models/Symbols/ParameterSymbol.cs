namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class ParameterSymbol : ISymbol
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public bool Ref { get; init; }

    public bool Out { get; init; }

    public bool Params { get; init; }

    public bool Optional { get; init; }

    public TypeSymbol? TypeSymbol { get; set; }

    public string? TypeQualifiedName { get; set; }
}