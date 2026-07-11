using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class ScopeResolver
{
    private readonly SymbolTable _semantic;

    public ScopeResolver(SymbolTable semantic)
    {
        _semantic = semantic;
    }
    public ISymbol? Resolve(MethodSymbol method, string name)
    {
        var scope = GetScope(method);

        if (scope.LocalVariables.TryGetValue(name, out var local))
            return local;

        if (scope.Parameters.TryGetValue(name, out var parameter))
            return parameter;

        if (scope.Fields.TryGetValue(name, out var field))
            return field;

        if (scope.Properties.TryGetValue(name, out var property))
            return property;

        return null;
    }
    public MethodScope GetScope(MethodSymbol method)
    {
        return _semantic.ScopesByMethodId[method.Id];
    }

    public LocalVariableSymbol? ResolveLocal(MethodSymbol method, string name)
    {
        var scope = GetScope(method);

        scope.LocalVariables.TryGetValue(name, out var symbol);

        return symbol;
    }

    public ParameterSymbol? ResolveParameter(MethodSymbol method, string name)
    {
        var scope = GetScope(method);

        scope.Parameters.TryGetValue(name, out var symbol);

        return symbol;
    }

    public FieldSymbol? ResolveField(MethodSymbol method, string name)
    {
        var scope = GetScope(method);

        scope.Fields.TryGetValue(name, out var symbol);

        return symbol;
    }

    public PropertySymbol? ResolveProperty(MethodSymbol method, string name)
    {
        var scope = GetScope(method);

        scope.Properties.TryGetValue(name, out var symbol);

        return symbol;
    }
    public ISymbol? ResolveVisibleSymbol(
        MethodSymbol method,
        string name)
    {
        var scope = GetScope(method);

        if (scope.LocalVariables.TryGetValue(name, out var local))
            return local;

        if (scope.Parameters.TryGetValue(name, out var parameter))
            return parameter;

        if (scope.Fields.TryGetValue(name, out var field))
            return field;

        if (scope.Properties.TryGetValue(name, out var property))
            return property;

        return null;
    }
}