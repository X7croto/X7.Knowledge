using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Graph;

public sealed class GraphQueryService : IGraphQueryService
{
    private readonly ProjectIndexOld _index;

    public GraphQueryService(ProjectIndexOld index)
    {
        _index = index;
    }

    public IEnumerable<MethodSymbol> GetCallers(MethodSymbol method)
        => method.Callers;

    public IEnumerable<MethodSymbol> GetCallees(MethodSymbol method)
        => method.Callees;

    public IEnumerable<TypeSymbol> GetDependencies(TypeSymbol type)
    {
        if (_index.Semantic.DependenciesBySource.TryGetValue(type, out var deps))
            return deps.Select(d => d.Target);

        return Enumerable.Empty<TypeSymbol>();
    }

    public IEnumerable<TypeSymbol> GetDependents(TypeSymbol type)
    {
        if (_index.Semantic.DependenciesByTarget.TryGetValue(type, out var deps))
            return deps.Select(d => d.Source);

        return Enumerable.Empty<TypeSymbol>();
    }

    public IEnumerable<MethodSymbol> GetReachableMethods(MethodSymbol root)
    {
        return GraphAlgorithms.DepthFirstSearch(
            root,
            x => x.Callees);
    }
    public IEnumerable<TypeSymbol> GetReachableTypes(TypeSymbol root)
    {
        return GraphAlgorithms.DepthFirstSearch(
            root,
            GetDependencies);
    }
    public IEnumerable<MethodSymbol> GetShortestMethodPath(MethodSymbol from, MethodSymbol to)
    {
        return GraphAlgorithms.ShortestPath(
            from,
            to,
            x => x.Callees);
    }

    public IEnumerable<MethodSymbol> GetAffectedMethods(MethodSymbol method)
    {
        return GraphAlgorithms.ReverseDepthFirstSearch(
            method,
            x => x.Callers);
    }

    public IEnumerable<TypeSymbol> GetAffectedTypes(TypeSymbol type)
    {
        return GraphAlgorithms.ReverseDepthFirstSearch(
            type,
            GetDependents);
    }
}