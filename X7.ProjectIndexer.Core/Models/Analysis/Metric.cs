namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class Metrics
{
    public int ProjectCount { get; set; }

    public int TypeCount { get; set; }

    public int MethodCount { get; set; }

    public int PropertyCount { get; set; }

    public int FieldCount { get; set; }

    public int ParameterCount { get; set; }

    public int LocalVariableCount { get; set; }

    public int CallCount { get; set; }

    public int DependencyCount { get; set; }

    public int CompositionCount { get; set; }

    public int AggregationCount { get; set; }

    public int InheritanceCount { get; set; }

    public int ImplementationCount { get; set; }
}