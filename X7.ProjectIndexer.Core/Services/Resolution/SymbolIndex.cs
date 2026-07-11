using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class SymbolIndex
{
    public Dictionary<string, TypeSymbol> TypesById { get; } = [];

    public Dictionary<string, TypeSymbol> TypesByName { get; } = [];

    public Dictionary<string, MethodSymbol> MethodsById { get; } = [];

    public Dictionary<string, List<MethodSymbol>> MethodsByName { get; } = [];

    public Dictionary<string, PropertySymbol> PropertiesById { get; } = [];

    public Dictionary<string, FieldSymbol> FieldsById { get; } = [];
}