using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Semantic;

namespace X7.ProjectIndexer.Core.Services.Semantic;

public sealed class SemanticIndexBuilder
{
    public void Build(SymbolTable semantic)
    {
        semantic.CallsByCaller.Clear();
        semantic.CallsByCallee.Clear();

        semantic.DependenciesBySource.Clear();
        semantic.DependenciesByTarget.Clear();

        foreach (var call in semantic.Calls)
        {
            if (!semantic.CallsByCaller.TryGetValue(call.Caller, out var callerList))
            {
                callerList = [];
                semantic.CallsByCaller[call.Caller] = callerList;
            }

            callerList.Add(call);

            if (call.Callee != null)
            {
                call.Caller.AddCallee(call.Callee);

                call.Callee.AddCaller(call.Caller);
            }

            if (call.Callee != null)
            {
                if (!semantic.CallsByCallee.TryGetValue(call.Callee, out var calleeList))
                {
                    calleeList = [];
                    semantic.CallsByCallee[call.Callee] = calleeList;
                }

                calleeList.Add(call);
            }
        }

        foreach (var dependency in semantic.Dependencies)
        {
            if (!semantic.DependenciesBySource.TryGetValue(dependency.Source, out var sourceList))
            {
                sourceList = [];
                semantic.DependenciesBySource[dependency.Source] = sourceList;
            }

            sourceList.Add(dependency);

            if (!semantic.DependenciesByTarget.TryGetValue(dependency.Target, out var targetList))
            {
                targetList = [];
                semantic.DependenciesByTarget[dependency.Target] = targetList;
            }

            targetList.Add(dependency);
        }
    }
}