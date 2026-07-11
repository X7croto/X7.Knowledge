namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class ModuleModel
{
    public required string Name { get; init; }

    public List<string> Types { get; } = [];

    public int Dependencies { get; set; }

    public int Dependents { get; set; }
}