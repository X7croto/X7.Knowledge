namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

public sealed class FeatureIndex
{
    public required string Name { get; init; }

    public List<string> Types { get; } = [];

    public List<string> Namespaces { get; } = [];
}