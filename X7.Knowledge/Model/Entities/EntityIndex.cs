namespace X7.Knowledge.Model.Entities;

/// <summary>
/// Projeção tipada sobre as Observations, oferecida por conveniência.
/// Construída exclusivamente a partir delas — nunca de fonte externa.
/// </summary>
public sealed class EntityIndex
{
    internal EntityIndex(
        Solution solution,
        IReadOnlyList<Project> projects,
        IReadOnlyList<SolutionFolder> folders)
    {
        Solution = solution;
        Projects = projects;
        Folders = folders;
    }

    public Solution Solution { get; }

    public IReadOnlyList<Project> Projects { get; }

    public IReadOnlyList<SolutionFolder> Folders { get; }
}
