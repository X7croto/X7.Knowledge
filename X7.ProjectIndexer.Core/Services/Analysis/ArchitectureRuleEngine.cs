using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class ArchitectureRuleEngine : IAnalyzer
{
    private readonly IReadOnlyList<IArchitectureRule> _rules =
    [
        new LayerDependencyRule()
    ];

    public void Analyze(ProjectIndexOld index)
    {
        index.Analysis.Violations.Clear();

        foreach (var rule in _rules)
        {
            rule.Analyze(index);
        }
    }
}