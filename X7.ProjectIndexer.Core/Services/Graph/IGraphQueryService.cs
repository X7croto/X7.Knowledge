using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Graph;

public interface IGraphQueryService
{
    IEnumerable<MethodSymbol> GetCallers(MethodSymbol method);

    IEnumerable<MethodSymbol> GetCallees(MethodSymbol method);

    IEnumerable<TypeSymbol> GetDependencies(TypeSymbol type);

    IEnumerable<TypeSymbol> GetDependents(TypeSymbol type);

    IEnumerable<MethodSymbol> GetReachableMethods(MethodSymbol root);

    IEnumerable<TypeSymbol> GetReachableTypes(TypeSymbol root);

    IEnumerable<MethodSymbol> GetShortestMethodPath(MethodSymbol from, MethodSymbol to);

    IEnumerable<MethodSymbol> GetAffectedMethods(MethodSymbol method);

    IEnumerable<TypeSymbol> GetAffectedTypes(TypeSymbol type);
}