using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class ConceptModel
{
    public required string Name { get; init; }

    public List<TypeSymbol> Types { get; } = [];

    public List<MethodSymbol> Methods { get; } = [];

    public List<string> Keywords { get; } = [];

    public List<string> Reasons { get; } = [];
}