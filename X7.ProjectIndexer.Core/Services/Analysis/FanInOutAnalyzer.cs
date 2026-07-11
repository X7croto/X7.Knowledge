using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class FanInOutAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        foreach (var method in index.Semantic.Methods)
        {
            method.FanIn = index.Semantic.CallsByCallee
                .TryGetValue(method, out var incoming)
                ? incoming.Count
                : 0;

            method.FanOut = index.Semantic.CallsByCaller
                .TryGetValue(method, out var outgoing)
                ? outgoing.Count
                : 0;
        }
    }
}