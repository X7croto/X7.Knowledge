namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class PropertySymbol : BaseSymbol
{
    public required string Type { get; init; }

    public bool HasGetter { get; init; }

    public bool HasSetter { get; init; }

    public bool InitOnly { get; init; }

    public string Accessibility { get; init; } = "";

    public TypeSymbol? DeclaringType { get; set; }

    public TypeSymbol? TypeSymbol { get; set; }

    public string? TypeQualifiedName { get; set; }
}