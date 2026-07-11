using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public sealed class DependencyRule : IProjectRule
{

    public string Name => "Dependency Inference";

    public void Analyze(ProjectIndexOld project, InferenceContext context)
    {
        var semantic = project.Semantic;
        var knowledge = project.Knowledge;

        foreach (var service in knowledge.Architecture.Services)
        {
            service.Dependencies.Clear();

            foreach (var dependency in semantic.Dependencies)
            {
                if (dependency.Source != service.Symbol)
                    continue;

                service.Dependencies.Add(dependency.Target.Name);
            }
        }
    }
}