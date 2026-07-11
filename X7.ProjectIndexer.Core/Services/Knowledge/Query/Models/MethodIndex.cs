namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

public sealed class MethodIndex
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Type { get; init; }

    public string Feature { get; set; } = "";

    public bool EntryPoint { get; set; }

    public bool Recursive { get; set; }

    public bool DeadCode { get; set; }

    public List<string> Calls { get; } = [];

    public List<string> CalledBy { get; } = [];
}