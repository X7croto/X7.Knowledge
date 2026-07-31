using X7.Knowledge.Acquisition;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C02 — observa referências declaradas: entre projetos e para pacotes
/// externos. Identidade e versão apenas; conteúdo de pacote não é resolvido.
/// </summary>
public sealed class ProjectReferenceProducer : IProducer
{
    public string Name => nameof(ProjectReferenceProducer);

    public string Capability => "C02";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        var declared = context.Solution.Projects
            .Select(p => p.RelativePath)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var entry in context.Solution.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Produce(context, entry, declared);
        }

        return ValueTask.CompletedTask;
    }

    private void Produce(
        CompilationContext context,
        ProjectEntry entry,
        HashSet<string> declaredProjects)
    {
        var projectId = KnowledgeId.ForProject(entry.RelativePath);

        var file = ProjectFileReader.Read(context.Solution.RootDirectory, entry.RelativePath);

        Provenance At(string? locator) => new()
        {
            Source = entry.RelativePath,
            Locator = locator,
            Producer = Name,
            Capability = Capability,
            AcquisitionLevel = context.AcquisitionLevel
        };

        foreach (var reference in file.ProjectReferences)
        {
            // Referência para fora da solução é fato observado, mas o alvo
            // não existe no modelo. Declarar a limitação e não criar aresta.
            if (!declaredProjects.Contains(reference))
            {
                context.Knowledge.Add(
                    ObservationKinds.AcquisitionLimitation,
                    projectId,
                    ObservationPayload.From(
                        ("reason", $"Referência para projeto fora da solução: '{reference}'"),
                        ("affectedScope", "project-reference")),
                    At(reference));

                continue;
            }

            context.Knowledge.Add(
                ObservationKinds.ProjectReferencesProject,
                projectId,
                ObservationPayload.From(
                    ("targetId", KnowledgeId.ForProject(reference).Value)),
                At(reference));
        }

        foreach (var package in file.PackageReferences)
        {
            context.Knowledge.Add(
                ObservationKinds.ProjectPackageReference,
                projectId,
                ObservationPayload.From(
                    ("name", package.Name),
                    ("version", package.Version)),
                At(package.Name));
        }
    }
}
