namespace X7.ProjectIndexer.Knowledge.ExportModels;

public sealed class MethodExport
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string DeclaringTypeId { get; init; }

    public int FanIn { get; init; }

    public int FanOut { get; init; }

    public bool Recursive { get; init; }

    public bool DeadCode { get; init; }

    public List<string> Calls { get; } = [];
}