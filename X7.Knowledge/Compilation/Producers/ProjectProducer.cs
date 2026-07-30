using X7.Knowledge.Acquisition;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>Observa cada projeto declarado na solução, lendo o .csproj.</summary>
public sealed class ProjectProducer : IProducer
{
    public string Name => nameof(ProjectProducer);

    public string Capability => "C01";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        foreach (var entry in context.Solution.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Produce(context, entry);
        }

        return ValueTask.CompletedTask;
    }

    private void Produce(CompilationContext context, ProjectEntry entry)
    {
        var projectId = KnowledgeId.ForProject(entry.RelativePath);

        var file = ProjectFileReader.Read(
            context.Solution.RootDirectory,
            entry.RelativePath);

        Provenance At(string? locator = null) => new()
        {
            Source = entry.RelativePath,
            Locator = locator,
            Producer = Name,
            Capability = Capability,
            AcquisitionLevel = context.AcquisitionLevel
        };

        context.Knowledge.Add(
            ObservationKinds.ProjectDeclared,
            projectId,
            ObservationPayload.From(
                ("name", entry.Name),
                ("relativePath", entry.RelativePath),
                ("directory", PathNormalizer.DirectoryOf(entry.RelativePath))),
            new Provenance
            {
                Source = context.Solution.FileName,
                Locator = entry.RelativePath,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = context.AcquisitionLevel
            });

        foreach (var framework in file.TargetFrameworks)
        {
            context.Knowledge.Add(
                ObservationKinds.ProjectTargetFramework,
                projectId,
                ObservationPayload.From(("moniker", framework)),
                At("TargetFramework"));
        }

        if (file.OutputKind is not null)
        {
            context.Knowledge.Add(
                ObservationKinds.ProjectOutputKind,
                projectId,
                ObservationPayload.From(("kind", file.OutputKind)),
                At("OutputType"));
        }

        if (file.LanguageVersion is not null)
        {
            context.Knowledge.Add(
                ObservationKinds.ProjectLanguageVersion,
                projectId,
                ObservationPayload.From(("version", file.LanguageVersion)),
                At("LangVersion"));
        }

        foreach (var property in file.Properties.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            context.Knowledge.Add(
                ObservationKinds.ProjectProperty,
                projectId,
                ObservationPayload.From(
                    ("name", property.Key),
                    ("value", property.Value)),
                At(property.Key));
        }

        if (file.IsTestProject)
        {
            context.Knowledge.Add(
                ObservationKinds.ProjectIsTestProject,
                projectId,
                ObservationPayload.From(("evidence", file.TestEvidence!)),
                At(file.TestEvidence));
        }

        foreach (var limitation in file.Limitations)
        {
            context.Knowledge.Add(
                ObservationKinds.AcquisitionLimitation,
                projectId,
                ObservationPayload.From(
                    ("reason", limitation.Reason),
                    ("affectedScope", limitation.AffectedScope)),
                new Provenance
                {
                    Source = limitation.Source,
                    Locator = limitation.Locator,
                    Producer = Name,
                    Capability = Capability,
                    AcquisitionLevel = context.AcquisitionLevel
                });
        }
    }
}
