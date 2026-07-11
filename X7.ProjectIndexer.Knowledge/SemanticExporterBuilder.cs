using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Knowledge.ExportModels;

namespace X7.ProjectIndexer.Knowledge;

public sealed class SemanticExportBuilder
{
    public SemanticExport Build(ProjectIndexOld index)
    {
        var model = new SemanticExport();

        foreach (var type in index.Semantic.Types)
        {
            model.Types.Add(new TypeExport
            {
                Id = type.Id,
                Name = type.Name,
                Namespace = type.Namespace,
                Kind = type.Kind,
                FanIn = type.FanIn,
                FanOut = type.FanOut,
                Instability = type.Instability,
                Abstractness = type.Abstractness,
                Distance = type.DistanceFromMainSequence,
                Layer = type.Layer
            });
        }

        foreach (var method in index.Semantic.Methods)
        {
            var export = new MethodExport
            {
                Id = method.Id,
                Name = method.Name,
                DeclaringTypeId = method.DeclaringType?.Id ?? "",
                FanIn = method.FanIn,
                FanOut = method.FanOut,
                Recursive = method.Recursive,
                DeadCode = method.IsDeadCode
            };

            foreach (var callee in method.Callees)
                export.Calls.Add(callee.Id);

            model.Methods.Add(export);
        }

        foreach (var dependency in index.Semantic.Dependencies)
        {
            model.Dependencies.Add(new DependencyExport
            {
                SourceId = dependency.Source.Id,
                TargetId = dependency.Target.Id
            });
        }

        return model;
    }
}