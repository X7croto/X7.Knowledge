using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Graph;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class DependencyChainQuery
{
    private readonly IGraphQueryService _graph;

    public DependencyChainQuery(IGraphQueryService graph)
    {
        _graph = graph;
    }

    public DependencyChain Analyze(
        TypeSymbol from,
        TypeSymbol to)
    {
        var result = new DependencyChain();

        foreach (var type in GraphAlgorithms.ShortestPath(
                     from,
                     to,
                     _graph.GetDependencies))
        {
            result.Add(type);
        }
        return result;
    }
}