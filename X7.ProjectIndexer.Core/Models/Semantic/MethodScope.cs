using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Semantic;

public sealed class MethodScope
{
    public required MethodSymbol Method { get; init; }

    public Dictionary<string, LocalVariableSymbol> LocalVariables { get; } = [];

    public Dictionary<string, ParameterSymbol> Parameters { get; } = [];

    public Dictionary<string, FieldSymbol> Fields { get; } = [];

    public Dictionary<string, PropertySymbol> Properties { get; } = [];
}