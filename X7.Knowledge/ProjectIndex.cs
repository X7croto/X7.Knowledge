internal sealed class ProjectIndex
{
    public required string SolutionName { get; init; }

    public required string SolutionPath { get; init; }

    public IReadOnlyList<ProjectIndexItem> Projects { get; init; }
        = [];
}