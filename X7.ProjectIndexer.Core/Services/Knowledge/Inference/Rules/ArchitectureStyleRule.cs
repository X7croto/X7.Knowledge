using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public sealed class ArchitectureStyleRule : IProjectRule
{

    public string Name => "Architecture Style Inference";

    public void Analyze(ProjectIndexOld index, InferenceContext context)
    {
        var semantic = index.Semantic;
        var architecture = index.Knowledge.Architecture;

        var services = index.Knowledge.Architecture.Services;

        var hasControllers =
            services.Any(s => s.Description.Kind == ServiceKind.Controller);

        var hasRepositories =
            services.Any(s => s.Description.Kind == ServiceKind.Repository);

        var hasHandlers =
            services.Any(s => s.Description.Kind == ServiceKind.Handler);

        if (hasControllers && hasRepositories)
        {
            index.Knowledge.Architecture.Patterns.Add(
                "Layered Architecture");
        }

        if (hasHandlers)
        {
            index.Knowledge.Architecture.Patterns.Add(
                "CQRS / Mediator");
        }
    }
}