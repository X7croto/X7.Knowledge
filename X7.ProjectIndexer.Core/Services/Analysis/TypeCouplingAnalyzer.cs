using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class TypeCouplingAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        foreach (var type in index.Semantic.Types)
        {
            type.FanIn =
                index.Semantic.DependenciesByTarget
                    .TryGetValue(type, out var incoming)
                        ? incoming.Count
                        : 0;

            type.FanOut =
                index.Semantic.DependenciesBySource
                    .TryGetValue(type, out var outgoing)
                        ? outgoing.Count
                        : 0;
        }
    }
}