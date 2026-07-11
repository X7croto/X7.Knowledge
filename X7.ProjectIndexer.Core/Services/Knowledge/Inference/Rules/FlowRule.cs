using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public sealed class FlowRule : IProjectRule
{
    public string Name => "Flow Inference";

    public void Analyze(ProjectIndexOld index, InferenceContext context)
    {
        var semantic = index.Semantic;

        foreach (var entry in semantic.Methods.Where(IsEntryPoint))
        {
            var flow = BuildFlow(index, entry);

            if (flow.Steps.Count > 0)
                index.Knowledge.Architecture.Flows.Add(flow);
        }
    }
    private static bool IsEntryPoint(MethodSymbol method)
    {
        if (method.DeclaringType == null)
            return false;

        var name = method.DeclaringType.Name;

        return name.EndsWith("Controller")
            || name.EndsWith("Endpoint")
            || method.IsEntryPoint;
    }
    private FlowModel BuildFlow(
        ProjectIndexOld index,
        MethodSymbol entry)
    {
        var flow = new FlowModel
        {
            Name = entry.Name
        };

        var visited = new HashSet<MethodSymbol>();

        Visit(entry, flow, visited, 0);

        return flow;
    }

    private void Visit(
        MethodSymbol method,
        FlowModel flow,
        HashSet<MethodSymbol> visited,
        int depth)
    {
        if (!visited.Add(method))
            return;

        flow.Steps.Add(new FlowStep
        {
            Method = method,
            Order = depth,
            Role = InferRole(method)
        });

        foreach (var next in method.Callees)
        {
            Visit(next, flow, visited, depth + 1);
        }
    }
    private static string InferRole(MethodSymbol method)
    {
        var type = method.DeclaringType;

        if (type == null)
            return "";

        var name = type.Name;

        if (name.EndsWith("Controller"))
            return "Controller";

        if (name.EndsWith("Service"))
            return "Service";

        if (name.EndsWith("Repository"))
            return "Repository";

        if (name.EndsWith("Handler"))
            return "Handler";

        return type.Kind;
    }
}