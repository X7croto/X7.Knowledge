using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;

namespace X7.ProjectIndexer.Core.Services.Knowledge;

public sealed class ServiceCompiler
{
    private readonly ServiceClassifier _classifier = new();

    public void Build(ProjectIndexOld index)
    {
        var services = index.Knowledge.Architecture.Services;

        services.Clear();

        foreach (var type in index.Semantic.Types)
        {
            var description = _classifier.Classify(type);

            if (description.Kind == ServiceKind.Unknown)
                continue;

            services.Add(new ServiceModel
            {
                Name = type.Name,
                Namespace = type.Namespace,
                Symbol = type,
                Description = description
            });
        }
    }
}