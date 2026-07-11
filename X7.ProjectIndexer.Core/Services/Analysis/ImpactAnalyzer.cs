using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class ImpactAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        index.Analysis.ImpactedMethods.Clear();

        foreach (var method in index.Semantic.Methods)
        {
            AnalyzeMethod(index, method);
        }
    }

    private void AnalyzeMethod(ProjectIndexOld index, MethodSymbol root)
    {
        var visited = new HashSet<MethodSymbol>();

        Visit(root, root, 0, visited, index);
    }

    private void Visit(
        MethodSymbol root,
        MethodSymbol current,
        int distance,
        HashSet<MethodSymbol> visited,
        ProjectIndexOld index)
    {
        if (!visited.Add(current))
            return;

        foreach (var call in index.Semantic.Calls)
        {
            if (call.Caller != current)
                continue;

            if (call.Callee is null)
                continue;

            index.Analysis.ImpactedMethods.Add(new ImpactedMethod
            {
                Source = root,
                Impacted = call.Callee,
                Distance = distance + 1
            });

            Visit(
                root,
                call.Callee,
                distance + 1,
                visited,
                index);
        }
    }
}