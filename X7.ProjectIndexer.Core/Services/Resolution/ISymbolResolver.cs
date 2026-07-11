using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Resolution;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public interface ISymbolResolver
{
    TypeSymbol? ResolveType(string name);

    TypeResolution ResolveTypeDetailed(string name);

    PropertySymbol? ResolveProperty(
        TypeSymbol type,
        string property);

    FieldSymbol? ResolveField(
        TypeSymbol type,
        string field);

    MethodSymbol? ResolveMethod(
        TypeSymbol type,
        string methodName);

    MethodSymbol? ResolveMethod(
        MethodSymbol caller,
        InvocationSymbol invocation);

    TypeSymbol? ResolveVariableType(
        MethodSymbol method,
        string variableName);
}