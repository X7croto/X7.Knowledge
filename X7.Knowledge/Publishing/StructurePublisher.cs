using System.Text;
using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>
/// C03 — publica um arquivo por projeto mais um índice enxuto.
/// </summary>
/// <remarks>
/// Projeção monolítica seria o pior caso para a métrica: qualquer pergunta
/// sobre um tipo pagaria a solução inteira. Um arquivo por projeto acompanha
/// a granularidade com que as perguntas são feitas e mantém cada arquivo em
/// tamanho comparável ao de `Structure/Solution.md`.
/// </remarks>
public sealed class StructurePublisher : IPublisher
{
    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        var types = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .ToArray();

        if (types.Length == 0)
            return;

        var byProject = types
            .GroupBy(o => KnowledgeId.Parse(o.Payload["projectId"]!))
            .ToDictionary(g => g.Key, g => g.ToArray());

        foreach (var project in model.Entities.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!byProject.TryGetValue(project.Id, out var projectTypes))
                continue;

            await CanonicalFile.WriteAsync(
                Path.Combine(outputDirectory, "Structure", "Types", $"{project.Name}.md"),
                BuildProject(model, project.Name, projectTypes),
                cancellationToken);
        }

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Structure", "Types", "INDEX.md"),
            BuildIndex(model, byProject),
            cancellationToken);

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Structure", "Namespaces.md"),
            BuildNamespaces(model),
            cancellationToken);
    }

    private static string LocationOf(KnowledgeModel model, KnowledgeId typeId)
        => model.Observations
            .FirstOrDefault(o => o.Kind == ObservationKinds.TypeLocation && o.Subject.Equals(typeId))
            ?.Payload["file"] ?? "—";

    private static string BuildProject(
        KnowledgeModel model,
        string projectName,
        IReadOnlyList<Observation> types)
    {
        var builder = new StringBuilder();

        builder.Append("# Tipos — ").Append(projectName).Append("\n\n");
        builder.Append(types.Count).Append(" tipo(s).\n\n");

        foreach (var group in types
                     .GroupBy(o => o.Payload["namespace"] ?? "(global)")
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            builder.Append("## ").Append(group.Key).Append("\n\n");
            builder.Append("| Tipo | Arquivo |\n|---|---|\n");

            foreach (var type in group.OrderBy(o => o.Payload["metadataName"], StringComparer.Ordinal))
            {
                builder.Append("| ").Append(type.Payload["name"])
                       .Append(" | `").Append(LocationOf(model, type.Subject))
                       .Append("` |\n");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static string BuildIndex(
        KnowledgeModel model,
        IReadOnlyDictionary<KnowledgeId, Observation[]> byProject)
    {
        var builder = new StringBuilder();

        builder.Append("# Índice de tipos\n\n");
        builder.Append("Um arquivo por projeto. Abra apenas o que precisar.\n\n");
        builder.Append("| Projeto | Tipos | Arquivo |\n|---|---|---|\n");

        foreach (var project in model.Entities.Projects)
        {
            if (!byProject.TryGetValue(project.Id, out var types))
                continue;

            builder.Append("| ").Append(project.Name)
                   .Append(" | ").Append(types.Length)
                   .Append(" | [`").Append(project.Name).Append(".md`](")
                   .Append(project.Name).Append(".md) |\n");
        }

        return builder.ToString();
    }

    private static string BuildNamespaces(KnowledgeModel model)
    {
        var builder = new StringBuilder();

        builder.Append("# Namespaces\n\n");

        var declared = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceDeclared)
            .OrderBy(o => o.Payload["name"], StringComparer.Ordinal)
            .ToArray();

        var counts = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceContains)
            .GroupBy(o => o.Subject)
            .ToDictionary(g => g.Key, g => g.Count());

        builder.Append("| Namespace | Tipos diretos |\n|---|---|\n");

        foreach (var observation in declared)
        {
            builder.Append("| ").Append(observation.Payload["name"])
                   .Append(" | ").Append(counts.GetValueOrDefault(observation.Subject, 0))
                   .Append(" |\n");
        }

        return builder.ToString();
    }
}
