using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class CycleAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        index.Analysis.Cycles.Clear();

        foreach (var method in index.Semantic.Methods)
        {
            Visit(
                index,
                method,
                new Stack<MethodSymbol>(),
                new HashSet<MethodSymbol>());
        }
    }

    private void Visit(
        ProjectIndexOld index,
        MethodSymbol current,
        Stack<MethodSymbol> stack,
        HashSet<MethodSymbol> visited)
    {
        if (stack.Contains(current))
        {
            var cycle = new Cycle();

            foreach (var method in stack.Reverse())
            {
                cycle.Methods.Add(method);

                if (method == current)
                    break;
            }

            foreach (var method in cycle.Methods)
                method.Recursive = true;

            cycle.Methods.Reverse();

            index.Analysis.Cycles.Add(cycle);

            return;
        }

        if (!visited.Add(current))
            return;

        stack.Push(current);

        foreach (var call in index.Semantic.Calls)
        {
            if (call.Caller != current)
                continue;

            if (call.Callee is null)
                continue;


            Visit(
                index,
                call.Callee,
                stack,
                visited);
        }

        stack.Pop();
    }
}