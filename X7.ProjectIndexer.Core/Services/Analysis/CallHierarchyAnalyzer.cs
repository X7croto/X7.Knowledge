using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class CallHierarchyAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        foreach (var method in index.Semantic.Methods)
        {
            method.MaxCallDepth = CalculateDepth(
                index,
                method,
                new HashSet<MethodSymbol>());
        }
    }

    private int CalculateDepth(
        ProjectIndexOld index,
        MethodSymbol method,
        HashSet<MethodSymbol> visited)
    {
        if (!visited.Add(method))
            return 0;

        if (!index.Semantic.CallsByCaller.TryGetValue(method, out var calls))
            return 0;

        var max = 0;

        foreach (var call in calls)
        {
            if (call.Callee is null)
                continue;

            max = Math.Max(
                max,
                1 + CalculateDepth(index, call.Callee, visited));
        }

        visited.Remove(method);

        return max;
    }
}