using System.Diagnostics;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class AnalysisPipeline
{
    private readonly IReadOnlyList<IAnalyzer> _analyzers =
    [
        new MetricsAnalyzer(),
        new FanInOutAnalyzer(),
        new TypeCouplingAnalyzer(),
        new InstabilityAnalyzer(),
        //new LayerAnalyzer(),
        new EntryPointAnalyzer(),
        new DeadCodeAnalyzer(),
        new ImpactAnalyzer(),
        new CycleAnalyzer(),
        new CallHierarchyAnalyzer(),
        new ArchitectureRuleEngine(),
    ];

    public void Analyze(ProjectIndexOld index)
    {
        foreach (var analyzer in _analyzers)
        {
            Console.WriteLine($"Analysis -> {analyzer.GetType().Name}");

            var sw = Stopwatch.StartNew();

            analyzer.Analyze(index);

            sw.Stop();

            Console.WriteLine(
                $"Analysis <- {analyzer.GetType().Name} ({sw.ElapsedMilliseconds} ms)");
        }
    }
}