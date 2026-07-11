using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class DeadCodeAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        index.Analysis.DeadMethods.Clear();

        var incoming = new Dictionary<MethodSymbol, int>();

        foreach (var method in index.Semantic.Methods)
            incoming[method] = 0;

        foreach (var call in index.Semantic.Calls)
        {
            if (call.Callee is null)
                continue;

            incoming[call.Callee]++;
        }

        foreach (var method in index.Semantic.Methods)
        {
            if (method.FanIn != 0)
                continue;

            method.IsDeadCode = true;

            index.Analysis.DeadMethods.Add(new DeadMethod
            {
                Method = method,
                IncomingCalls = 0
            });
        }
    }
}