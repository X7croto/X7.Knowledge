using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Graph;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class ImpactQueryService
{
    private readonly GraphQueryService _graph;

    public ImpactQueryService(ProjectIndexOld index)
    {
        _graph = new GraphQueryService(index);
    }

    public ImpactQuery Analyze(MethodSymbol method)
    {
        var result = new ImpactQuery
        {
            Root = method
        };

        foreach (var affected in _graph.GetReachableMethods(method))
        {
            result.AffectedMethods.Add(affected);

            if (affected.DeclaringType is not null &&
                !result.AffectedTypes.Contains(affected.DeclaringType))
            {
                result.AffectedTypes.Add(affected.DeclaringType);
            }

            result.Level =
                result.TypeCount switch
                {
                    < 5 => ImpactLevel.Low,

                    < 20 => ImpactLevel.Medium,

                    < 50 => ImpactLevel.High,

                    _ => ImpactLevel.Critical
                };
        }

        return result;
    }
}