using System.Text;
using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>
/// Projeção legível: README.md e Structure/Solution.md.
/// Não calcula nada — apenas materializa o que já está no modelo (PR-06).
/// </summary>
public sealed class MarkdownPublisher : IPublisher
{
    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "README.md"),
            BuildReadme(model),
            cancellationToken);

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Structure", "Solution.md"),
            BuildSolution(model),
            cancellationToken);
    }

    private static string BuildReadme(KnowledgeModel model)
    {
        var manifest = model.Manifest;
        var builder = new StringBuilder();

        builder.Append("# Base de Conhecimento — ")
               .Append(model.Entities.Solution.Name)
               .Append("\n\n");

        builder.Append("Gerada por X7.Knowledge. Conteúdo derivado deterministicamente do código-fonte.\n\n");

        builder.Append("## Manifesto\n\n");
        builder.Append("| Campo | Valor |\n|---|---|\n");
        builder.Append($"| Versão do modelo | `{manifest.ModelVersion}` |\n");
        builder.Append($"| Versão do compilador | `{manifest.CompilerVersion}` |\n");
        builder.Append($"| Solução | `{manifest.SolutionId.Value}` |\n");
        builder.Append($"| Nível de aquisição | `{manifest.AcquisitionLevel.ToToken()}` |\n");
        builder.Append($"| Capacidades | {string.Join(", ", manifest.Capabilities)} |\n");
        builder.Append($"| Digest das entradas | `{manifest.InputDigest}` |\n");
        builder.Append($"| Observations | {manifest.ObservationCount} |\n\n");

        if (manifest.AcquisitionLevel == AcquisitionLevel.Syntactic)
        {
            builder.Append("> Nível **X (sintático)**: nenhuma relação semântica foi resolvida. ");
            builder.Append("Relações entre tipos não estão disponíveis nesta capacidade.\n\n");
        }

        builder.Append("## Conteúdo\n\n");
        builder.Append("- [`Structure/Solution.md`](Structure/Solution.md) — solução, projetos, frameworks e árvore lógica\n");
        builder.Append("- `model/knowledge.model.json` — forma canônica do KnowledgeModel\n");

        var limitations = model.Observations
            .Where(o => o.Kind == ObservationKinds.AcquisitionLimitation)
            .ToArray();

        if (limitations.Length > 0)
        {
            builder.Append("\n## Limitações declaradas\n\n");
            builder.Append($"{limitations.Length} item(ns) que o compilador não conseguiu obter. ");
            builder.Append("Ausência silenciosa é proibida: ver `Structure/Solution.md`.\n");
        }

        return builder.ToString();
    }

    private static string BuildSolution(KnowledgeModel model)
    {
        var solution = model.Entities.Solution;
        var builder = new StringBuilder();

        builder.Append("# Estrutura Física — ").Append(solution.Name).Append("\n\n");

        builder.Append("## Projetos\n\n");
        builder.Append("| Projeto | Caminho | Frameworks | Saída | Teste |\n");
        builder.Append("|---|---|---|---|---|\n");

        foreach (var project in model.Entities.Projects)
        {
            builder.Append("| ").Append(project.Name)
                   .Append(" | `").Append(project.RelativePath)
                   .Append("` | ").Append(project.TargetFrameworks.Count == 0
                       ? "—"
                       : string.Join(", ", project.TargetFrameworks))
                   .Append(" | ").Append(project.OutputKind ?? "—")
                   .Append(" | ").Append(project.IsTestProject == true ? "sim" : "—")
                   .Append(" |\n");
        }

        builder.Append("\n## Árvore lógica\n\n```\n");
        AppendTree(builder, model);
        builder.Append("```\n");

        AppendLimitations(builder, model);

        return builder.ToString();
    }

    private static void AppendTree(StringBuilder builder, KnowledgeModel model)
    {
        var solution = model.Entities.Solution;

        builder.Append(solution.Name).Append('\n');

        var roots = model.Entities.Folders
            .Where(f => f.Parent is null)
            .OrderBy(f => f.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (var folder in roots)
            AppendFolder(builder, model, folder.Id, indent: 1);

        var foldered = model.Entities.Folders
            .SelectMany(f => f.Children)
            .ToHashSet();

        var loose = solution.Projects
            .Where(p => !foldered.Contains(p))
            .OrderBy(p => p)
            .ToArray();

        foreach (var projectId in loose)
            AppendProject(builder, model, projectId, indent: 1);
    }

    private static void AppendFolder(
        StringBuilder builder,
        KnowledgeModel model,
        KnowledgeId folderId,
        int indent)
    {
        var folder = model.Entities.Folders.First(f => f.Id.Equals(folderId));

        builder.Append(new string(' ', indent * 2))
               .Append(folder.Name)
               .Append("/\n");

        foreach (var child in folder.Children)
        {
            if (child.Value.StartsWith("slnfolder:", StringComparison.Ordinal))
                AppendFolder(builder, model, child, indent + 1);
            else
                AppendProject(builder, model, child, indent + 1);
        }
    }

    private static void AppendProject(
        StringBuilder builder,
        KnowledgeModel model,
        KnowledgeId projectId,
        int indent)
    {
        var project = model.Entities.Projects.FirstOrDefault(p => p.Id.Equals(projectId));

        builder.Append(new string(' ', indent * 2))
               .Append(project?.Name ?? projectId.Value)
               .Append('\n');
    }

    private static void AppendLimitations(StringBuilder builder, KnowledgeModel model)
    {
        var limitations = model.Observations
            .Where(o => o.Kind == ObservationKinds.AcquisitionLimitation)
            .ToArray();

        if (limitations.Length == 0)
            return;

        builder.Append("\n## Limitações de aquisição\n\n");
        builder.Append("O que o compilador não conseguiu obter, declarado explicitamente.\n\n");
        builder.Append("| Escopo | Motivo | Origem |\n|---|---|---|\n");

        foreach (var limitation in limitations)
        {
            builder.Append("| ").Append(limitation.Payload["affectedScope"])
                   .Append(" | ").Append(limitation.Payload["reason"])
                   .Append(" | `").Append(limitation.Provenance.Source)
                   .Append("` |\n");
        }
    }
}
