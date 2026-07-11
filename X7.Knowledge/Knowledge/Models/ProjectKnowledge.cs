public sealed class ProjectKnowledge
{
    public required string Name { get; init; }

    public IReadOnlyList<ProjectSummary> Projects { get; init; }
        = [];
}