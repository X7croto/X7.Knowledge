using X7.ProjectIndexer.Core.Models.Knowledge;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public sealed class LayerRule : ITypeRule
{
    public string Name => "Layer Inference";

    public void Analyze(TypeSymbol type, InferenceContext context)
    {
        var index = context.Symbols.Index;

        foreach (var service in index.Knowledge.Architecture.Services)
        {
            service.Layer = InferLayer(service);
        }
    }

    private static string InferLayer(ServiceModel service)
    {
        var ns = service.Namespace;

        if (ns.Contains(".Presentation"))
            return "Presentation";

        if (ns.Contains(".Application"))
            return "Application";

        if (ns.Contains(".Domain"))
            return "Domain";

        if (ns.Contains(".Infrastructure"))
            return "Infrastructure";

        if (ns.Contains(".Persistence"))
            return "Persistence";

        return "Unknown";
    }
}