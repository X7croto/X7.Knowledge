namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class FieldSymbol : BaseSymbol
{
    public required string Type { get; init; }

    public string Accessibility { get; init; } = "";

    public bool Static { get; init; }

    public bool Readonly { get; init; }

    public bool Const { get; init; }

    public TypeSymbol? DeclaringType { get; set; }

    public string? TypeQualifiedName { get; set; }

    public TypeSymbol? TypeSymbol { get; set; }
}