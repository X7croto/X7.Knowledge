namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

public sealed class TypeIndex
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Namespace { get; init; }

    public string Layer { get; set; } = "";

    public string Feature { get; set; } = "";

    public bool IsService { get; set; }

    public bool IsController { get; set; }

    public bool IsRepository { get; set; }

    public bool IsEntity { get; set; }

    public List<string> Methods { get; } = [];

    public List<string> Dependencies { get; } = [];

    public List<string> Dependents { get; } = [];

    public List<string> Implements { get; } = [];

    public List<string> Inherits { get; } = [];
}