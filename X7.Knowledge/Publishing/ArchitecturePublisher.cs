using System.Text;
using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>
/// C02 — projeção arquitetural. Materializa Observations e Inferences
/// já existentes; não calcula posição, camada nem ciclo (PR-06).
/// </summary>
public sealed class ArchitecturePublisher : IPublisher
{
    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        if (model.Inferences.Count == 0)
            return;

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Architecture", "Architecture.md"),
            BuildArchitecture(model),
            cancellationToken);

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Architecture", "ProjectDependencies.md"),
            BuildDependencies(model),
            cancellationToken);
    }

    private static string NameOf(KnowledgeModel model, KnowledgeId id)
        => model.Entities.Projects.FirstOrDefault(p => p.Id.Equals(id))?.Name ?? id.Value;

    private static string BuildArchitecture(KnowledgeModel model)
    {
        var builder = new StringBuilder();

        builder.Append("# Arquitetura — ").Append(model.Entities.Solution.Name).Append("\n\n");
        builder.Append("Derivado do grafo de dependências entre projetos. ");
        builder.Append("Toda afirmação abaixo aponta sua Evidence no KnowledgeModel.\n\n");

        var layers = model.Inferences
            .Where(i => i.Kind == InferenceKinds.ProjectLayer)
            .GroupBy(i => int.Parse(i.Payload["depth"]!))
            .OrderBy(g => g.Key)
            .ToArray();

        builder.Append("## Camadas\n\n");
        builder.Append("Profundidade é a maior distância até um projeto que não referencia ");
        builder.Append("nenhum outro da solução. Regra `layer-by-graph-depth`, Confidence `Asserted`.\n\n");

        foreach (var layer in layers)
        {
            builder.Append("### Camada ").Append(layer.Key).Append("\n\n");

            foreach (var inference in layer.OrderBy(i => i.Subject))
                builder.Append("- ").Append(NameOf(model, inference.Subject)).Append('\n');

            builder.Append('\n');
        }

        AppendFlagged(builder, model, InferenceKinds.ProjectIsRoot,
            "## Projetos-raiz",
            "Nenhum projeto da solução depende deles. Regra `root-by-absence-of-dependents`.");

        AppendFlagged(builder, model, InferenceKinds.ProjectIsLeaf,
            "## Projetos-folha",
            "Não referenciam nenhum projeto da solução. Regra `leaf-by-absence-of-references`.");

        AppendCycles(builder, model);

        return builder.ToString();
    }

    private static void AppendFlagged(
        StringBuilder builder,
        KnowledgeModel model,
        string kind,
        string title,
        string explanation)
    {
        var inferences = model.Inferences
            .Where(i => i.Kind == kind)
            .OrderBy(i => i.Subject)
            .ToArray();

        if (inferences.Length == 0)
            return;

        builder.Append(title).Append("\n\n").Append(explanation).Append("\n\n");

        foreach (var inference in inferences)
            builder.Append("- ").Append(NameOf(model, inference.Subject)).Append('\n');

        builder.Append('\n');
    }

    private static void AppendCycles(StringBuilder builder, KnowledgeModel model)
    {
        var cycles = model.Inferences
            .Where(i => i.Kind == InferenceKinds.ProjectParticipatesInCycle)
            .GroupBy(i => i.Payload["cycleId"]!)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToArray();

        builder.Append("## Ciclos de dependência\n\n");

        if (cycles.Length == 0)
        {
            builder.Append("Nenhum ciclo entre projetos.\n");
            return;
        }

        foreach (var cycle in cycles)
        {
            builder.Append("- `").Append(cycle.Key).Append("`: ");
            builder.Append(string.Join(", ", cycle
                .OrderBy(i => i.Subject)
                .Select(i => NameOf(model, i.Subject))));
            builder.Append('\n');
        }
    }

    private static string BuildDependencies(KnowledgeModel model)
    {
        var builder = new StringBuilder();

        builder.Append("# Dependências entre projetos\n\n");

        var references = model.Observations
            .Where(o => o.Kind == ObservationKinds.ProjectReferencesProject)
            .ToArray();

        builder.Append("## Referências declaradas\n\n");

        if (references.Length == 0)
        {
            builder.Append("Nenhuma referência entre projetos da solução.\n\n");
        }
        else
        {
            builder.Append("| Projeto | Referencia |\n|---|---|\n");

            foreach (var group in references
                         .GroupBy(o => o.Subject)
                         .OrderBy(g => g.Key))
            {
                builder.Append("| ").Append(NameOf(model, group.Key)).Append(" | ");
                builder.Append(string.Join(", ", group
                    .Select(o => KnowledgeId.Parse(o.Payload["targetId"]!))
                    .OrderBy(id => id)
                    .Select(id => NameOf(model, id))));
                builder.Append(" |\n");
            }

            builder.Append('\n');
        }

        builder.Append("## Quem depende de quem\n\n");
        builder.Append("| Projeto | É referenciado por |\n|---|---|\n");

        foreach (var project in model.Entities.Projects)
        {
            var dependents = references
                .Where(o => KnowledgeId.Parse(o.Payload["targetId"]!).Equals(project.Id))
                .Select(o => o.Subject)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();

            builder.Append("| ").Append(project.Name).Append(" | ");
            builder.Append(dependents.Length == 0
                ? "—"
                : string.Join(", ", dependents.Select(id => NameOf(model, id))));
            builder.Append(" |\n");
        }

        AppendPackages(builder, model);

        return builder.ToString();
    }

    private static void AppendPackages(StringBuilder builder, KnowledgeModel model)
    {
        var packages = model.Observations
            .Where(o => o.Kind == ObservationKinds.ProjectPackageReference)
            .ToArray();

        if (packages.Length == 0)
            return;

        builder.Append("\n## Pacotes externos declarados\n\n");
        builder.Append("Identidade e versão apenas; conteúdo não é resolvido.\n\n");
        builder.Append("| Projeto | Pacote | Versão |\n|---|---|---|\n");

        foreach (var observation in packages
                     .OrderBy(o => o.Subject)
                     .ThenBy(o => o.Payload["name"], StringComparer.Ordinal))
        {
            builder.Append("| ").Append(NameOf(model, observation.Subject))
                   .Append(" | ").Append(observation.Payload["name"])
                   .Append(" | ").Append(observation.Payload["version"] ?? "não resolvida")
                   .Append(" |\n");
        }
    }
}
