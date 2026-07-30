namespace X7.Knowledge.Model.Entities;

public sealed record Project
{
    public required KnowledgeId Id { get; init; }

    public required string Name { get; init; }

    public required string RelativePath { get; init; }

    public required string Directory { get; init; }

    public required IReadOnlyList<string> TargetFrameworks { get; init; }

    public string? OutputKind { get; init; }

    public string? LanguageVersion { get; init; }

    public bool? IsTestProject { get; init; }
}
