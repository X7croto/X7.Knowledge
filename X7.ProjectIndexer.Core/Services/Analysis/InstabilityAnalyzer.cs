using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class InstabilityAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        index.Analysis.TypeMetrics.Clear();

        foreach (var type in index.Semantic.Types)
        {
            var ca = type.FanIn;
            var ce = type.FanOut;

            var instability =
                (ca + ce) == 0
                    ? 0
                    : (double)ce / (ca + ce);

            var abstractness = CalculateAbstractness(type);

            var distance = Math.Abs(abstractness + instability - 1);

            type.Abstractness = abstractness;

            type.DistanceFromMainSequence = distance;

            index.Analysis.TypeMetrics.Add(
                new TypeMetrics
                {
                    Type = type,

                    Layer = type.Layer,

                    AfferentCoupling = ca,

                    EfferentCoupling = ce,

                    Instability = instability,

                    Abstractness = abstractness,

                    Distance = distance
                });
        }
    }

    private static double CalculateAbstractness(TypeSymbol type)
    {
        if (type.Kind == "Interface")
            return 1;

        if (type.Abstract)
            return 1;

        return 0;
    }
}