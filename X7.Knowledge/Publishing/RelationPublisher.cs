using System.Text;
using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>
/// C04 — herança e implementação, em arquivo próprio, um por projeto.
/// </summary>
/// <remarks>
/// Separado do inventário de tipos de propósito. Numa tabela única, toda
/// pergunta sobre localização de tipo pagaria também pelas relações, que não
/// usa — foi o que o benchmark mediu: Q07 saltou de 5410‰ para 6731‰ quando
/// as colunas foram acrescentadas ao inventário. Modelo §9.1: partição pela
/// unidade natural de consulta.
/// </remarks>
public sealed class RelationPublisher : IPublisher
{
    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        var relations = model.Observations
            .Where(o => o.Kind is ObservationKinds.TypeInherits or ObservationKinds.TypeImplements)
            .ToArray();

        if (relations.Length == 0)
            return;

        var projectOf = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .ToDictionary(o => o.Subject, o => KnowledgeId.Parse(o.Payload["projectId"]!));

        var byProject = relations
            .Where(o => projectOf.ContainsKey(o.Subject))
            .GroupBy(o => projectOf[o.Subject])
            .ToDictionary(g => g.Key, g => g.ToArray());

        var publicados = new List<string>();

        foreach (var project in model.Entities.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!byProject.TryGetValue(project.Id, out var doProjeto))
                continue;

            await CanonicalFile.WriteAsync(
                Path.Combine(outputDirectory, "Relations", $"{project.Name}.md"),
                Build(model, project.Name, doProjeto),
                cancellationToken);

            publicados.Add(project.Name);
        }

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Relations", "INDEX.md"),
            BuildIndex(publicados),
            cancellationToken);
    }

    private static string NameOf(KnowledgeModel model, KnowledgeId typeId)
        => model.Observations
            .FirstOrDefault(o => o.Kind == ObservationKinds.TypeDeclared && o.Subject.Equals(typeId))
            ?.Payload["name"] ?? typeId.Value;

    private static string Short(string qualifiedName)
    {
        var generic = qualifiedName.IndexOf('<', StringComparison.Ordinal);

        var head = generic < 0 ? qualifiedName : qualifiedName[..generic];

        var dot = head.LastIndexOf('.');

        return dot < 0 ? qualifiedName : qualifiedName[(dot + 1)..];
    }

    private static string Build(
        KnowledgeModel model,
        string projectName,
        IReadOnlyList<Observation> relations)
    {
        var builder = new StringBuilder();

        builder.Append("# Relações de tipo — ").Append(projectName).Append("\n\n");
        builder.Append("Herança e implementação declaradas diretamente. ");
        builder.Append("Interface herdada da classe base não aparece: é derivável destas.\n\n");

        builder.Append("| Tipo | Herda de | Implementa |\n|---|---|---|\n");

        foreach (var typeId in relations
                     .Select(o => o.Subject)
                     .Distinct()
                     .OrderBy(id => id))
        {
            var herda = relations
                .FirstOrDefault(o => o.Kind == ObservationKinds.TypeInherits && o.Subject.Equals(typeId));

            var implementa = relations
                .Where(o => o.Kind == ObservationKinds.TypeImplements && o.Subject.Equals(typeId))
                .Select(o => Short(o.Payload["interfaceName"]!))
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            builder.Append("| ").Append(NameOf(model, typeId))
                   .Append(" | ").Append(herda is null ? "—" : Short(herda.Payload["baseTypeName"]!))
                   .Append(" | ").Append(implementa.Length == 0 ? "—" : string.Join(", ", implementa))
                   .Append(" |\n");
        }

        return builder.ToString();
    }

    private static string BuildIndex(IReadOnlyList<string> projects)
    {
        var builder = new StringBuilder();

        builder.Append("# Índice de relações\n\n");
        builder.Append("Um arquivo por projeto. Abra apenas o que precisar.\n\n");
        builder.Append("| Projeto | Arquivo |\n|---|---|\n");

        foreach (var project in projects)
        {
            builder.Append("| ").Append(project)
                   .Append(" | [`").Append(project).Append(".md`](")
                   .Append(project).Append(".md) |\n");
        }

        return builder.ToString();
    }
}
