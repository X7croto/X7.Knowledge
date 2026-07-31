using X7.Knowledge.Acquisition;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>Observa a solução: identidade, projetos contidos e pastas lógicas.</summary>
public sealed class SolutionProducer : IProducer
{
    public string Name => nameof(SolutionProducer);

    public string Capability => "C01";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        var solution = context.Solution;
        var solutionId = context.SolutionId;

        Provenance At(string? locator = null) => new()
        {
            Source = solution.FileName,
            Locator = locator,
            Producer = Name,
            Capability = Capability,
            // Este Producer lê apenas arquivos de projeto: nada aqui
            // depende de semântica, mesmo quando ela está disponível.
            AcquisitionLevel = AcquisitionLevel.Syntactic
        };

        context.Knowledge.Add(
            ObservationKinds.SolutionDeclared,
            solutionId,
            ObservationPayload.From(("name", solution.Name)),
            At());

        foreach (var project in solution.Projects)
        {
            var projectId = KnowledgeId.ForProject(project.RelativePath);

            context.Knowledge.Add(
                ObservationKinds.SolutionContainsProject,
                solutionId,
                ObservationPayload.From(("projectId", projectId.Value)),
                At(project.RelativePath));
        }

        foreach (var folder in solution.Folders)
        {
            var folderId = KnowledgeId.ForSolutionFolder(folder.LogicalPath);

            var parentId = folder.ParentLogicalPath is null
                ? null
                : KnowledgeId.ForSolutionFolder(folder.ParentLogicalPath).Value;

            context.Knowledge.Add(
                ObservationKinds.SolutionFolder,
                folderId,
                ObservationPayload.From(
                    ("name", folder.Name),
                    ("parentId", parentId)),
                At(folder.LogicalPath));
        }

        // Pasta contém pasta.
        foreach (var folder in solution.Folders.Where(f => f.ParentLogicalPath is not null))
        {
            var parentId = KnowledgeId.ForSolutionFolder(folder.ParentLogicalPath!);
            var childId = KnowledgeId.ForSolutionFolder(folder.LogicalPath);

            context.Knowledge.Add(
                ObservationKinds.SolutionFolderContains,
                parentId,
                ObservationPayload.From(("childId", childId.Value)),
                At(folder.LogicalPath));
        }

        // Pasta contém projeto.
        foreach (var project in solution.Projects.Where(p => p.FolderLogicalPath is not null))
        {
            var folderId = KnowledgeId.ForSolutionFolder(project.FolderLogicalPath!);
            var projectId = KnowledgeId.ForProject(project.RelativePath);

            context.Knowledge.Add(
                ObservationKinds.SolutionFolderContains,
                folderId,
                ObservationPayload.From(("childId", projectId.Value)),
                At(project.RelativePath));
        }

        foreach (var limitation in solution.Limitations)
        {
            context.Knowledge.Add(
                ObservationKinds.AcquisitionLimitation,
                solutionId,
                ObservationPayload.From(
                    ("reason", limitation.Reason),
                    ("affectedScope", limitation.AffectedScope)),
                At(limitation.Locator));
        }

        return ValueTask.CompletedTask;
    }
}
