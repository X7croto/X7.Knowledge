using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class LayerDependencyRule : IArchitectureRule
{
    public void Analyze(ProjectIndexOld index)
    {
        foreach (var dependency in index.Semantic.Dependencies)
        {
            if (dependency.Source.Layer <= dependency.Target.Layer)
                continue;

            index.Analysis.Violations.Add(
                new ArchitecturalViolation
                {
                    Rule = "LayerDependency",

                    Message =
                        $"{dependency.Source.Name} depende de uma camada superior.",

                    SourceType = dependency.Source,

                    TargetType = dependency.Target
                });
        }
    }
}