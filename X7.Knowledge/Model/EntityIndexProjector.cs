using X7.Knowledge.Model.Entities;

namespace X7.Knowledge.Model;

/// <summary>
/// Deriva as entidades tipadas exclusivamente das Observations.
/// É a garantia estrutural de IV-02: nenhum campo aparece sem origem observada.
/// </summary>
internal static class EntityIndexProjector
{
    public static EntityIndex Project(IReadOnlyList<Observation> observations)
    {
        var solutionId = observations
            .Where(o => o.Kind == ObservationKinds.SolutionDeclared)
            .Select(o => (KnowledgeId?)o.Subject)
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Nenhuma observação 'solution.declared'. Modelo inválido.");

        var solutionName = Single(observations, ObservationKinds.SolutionDeclared, solutionId, "name")
            ?? throw new InvalidOperationException("Solução sem nome observado.");

        var projectIds = observations
            .Where(o => o.Kind == ObservationKinds.SolutionContainsProject)
            .Select(o => KnowledgeId.Parse(o.Payload["projectId"]!))
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var folderIds = observations
            .Where(o => o.Kind == ObservationKinds.SolutionFolder)
            .Select(o => o.Subject)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var solution = new Solution
        {
            Id = solutionId,
            Name = solutionName,
            Projects = projectIds,
            Folders = folderIds
        };

        var projects = observations
            .Where(o => o.Kind == ObservationKinds.ProjectDeclared)
            .Select(o => o.Subject)
            .Distinct()
            .OrderBy(id => id)
            .Select(id => ProjectFrom(observations, id))
            .ToArray();

        var folders = folderIds
            .Select(id => FolderFrom(observations, id))
            .ToArray();

        return new EntityIndex(solution, projects, folders);
    }

    private static Project ProjectFrom(
        IReadOnlyList<Observation> observations,
        KnowledgeId id)
    {
        var declared = observations.First(o =>
            o.Kind == ObservationKinds.ProjectDeclared && o.Subject.Equals(id));

        var frameworks = observations
            .Where(o => o.Kind == ObservationKinds.ProjectTargetFramework
                        && o.Subject.Equals(id))
            .Select(o => o.Payload["moniker"]!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToArray();

        var isTest = observations.Any(o =>
            o.Kind == ObservationKinds.ProjectIsTestProject && o.Subject.Equals(id));

        return new Project
        {
            Id = id,
            Name = declared.Payload["name"]!,
            RelativePath = declared.Payload["relativePath"]!,
            Directory = declared.Payload["directory"]!,
            TargetFrameworks = frameworks,
            OutputKind = Single(observations, ObservationKinds.ProjectOutputKind, id, "kind"),
            LanguageVersion = Single(observations, ObservationKinds.ProjectLanguageVersion, id, "version"),
            IsTestProject = isTest ? true : null
        };
    }

    private static SolutionFolder FolderFrom(
        IReadOnlyList<Observation> observations,
        KnowledgeId id)
    {
        var declared = observations.First(o =>
            o.Kind == ObservationKinds.SolutionFolder && o.Subject.Equals(id));

        var parentRaw = declared.Payload["parentId"];

        var children = observations
            .Where(o => o.Kind == ObservationKinds.SolutionFolderContains
                        && o.Subject.Equals(id))
            .Select(o => KnowledgeId.Parse(o.Payload["childId"]!))
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        return new SolutionFolder
        {
            Id = id,
            Name = declared.Payload["name"]!,
            Parent = parentRaw is null ? null : KnowledgeId.Parse(parentRaw),
            Children = children
        };
    }

    private static string? Single(
        IReadOnlyList<Observation> observations,
        string kind,
        KnowledgeId subject,
        string payloadKey)
        => observations
            .Where(o => o.Kind == kind && o.Subject.Equals(subject))
            .Select(o => o.Payload[payloadKey])
            .FirstOrDefault();
}
