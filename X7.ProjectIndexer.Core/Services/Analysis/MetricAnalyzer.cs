using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class MetricsAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        var semantic = index.Semantic;
        var metrics = index.Analysis.Metrics;


        metrics.PropertyCount = semantic.Properties.Count;

        metrics.FieldCount = semantic.Fields.Count;

        metrics.ParameterCount = semantic.Parameters.Count;

        metrics.LocalVariableCount = semantic.LocalVariables.Count;

        metrics.CompositionCount = semantic.Compositions.Count;

        metrics.AggregationCount = semantic.Aggregations.Count;

        metrics.InheritanceCount = semantic.Inheritances.Count;

        metrics.ImplementationCount = semantic.Implementations.Count;
        
        metrics.ProjectCount = semantic.Projects.Count;

        metrics.TypeCount = semantic.Types.Count;

        metrics.MethodCount = semantic.Methods.Count;

        metrics.DependencyCount = semantic.Dependencies.Count;

        metrics.CallCount = semantic.Calls.Count;
    }
}