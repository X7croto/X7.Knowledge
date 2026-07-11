namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

public sealed class NamespaceIndex
{
    public required string Name { get; init; }

    public List<string> Types { get; } = [];

    public List<string> Features { get; } = [];
}