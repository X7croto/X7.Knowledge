using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public sealed class FeatureRule : ITypeRule
{
    public string Name => "Feature Inference";
    public void Analyze(TypeSymbol type, InferenceContext context)
    {
        var index = context.Symbols.Index;

        foreach (var flow in index.Knowledge.Architecture.Flows)
        {
            var feature = Build(flow);

            index.Knowledge.Architecture.Features.Add(feature);
        }
    }

    private static FeatureModel Build(FlowModel flow)
    {
        var feature = new FeatureModel
        {
            Name = flow.Name
        };

        feature.Flows.Add(flow);

        foreach (var step in flow.Steps)
        {
            feature.Methods.Add(step.Method);

            if (step.Method.DeclaringType != null &&
                !feature.Types.Contains(step.Method.DeclaringType))
            {
                feature.Types.Add(step.Method.DeclaringType);
            }
        }

        feature.Reasons.Add("Derived from execution flow.");

        return feature;
    }
}