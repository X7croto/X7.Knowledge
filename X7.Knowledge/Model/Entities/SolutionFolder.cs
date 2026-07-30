namespace X7.Knowledge.Model.Entities;

public sealed record SolutionFolder
{
    public required KnowledgeId Id { get; init; }

    public required string Name { get; init; }

    public KnowledgeId? Parent { get; init; }

    public required IReadOnlyList<KnowledgeId> Children { get; init; }
}
