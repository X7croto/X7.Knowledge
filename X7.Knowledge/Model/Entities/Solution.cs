namespace X7.Knowledge.Model.Entities;

public sealed record Solution
{
    public required KnowledgeId Id { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyList<KnowledgeId> Projects { get; init; }

    public required IReadOnlyList<KnowledgeId> Folders { get; init; }
}
