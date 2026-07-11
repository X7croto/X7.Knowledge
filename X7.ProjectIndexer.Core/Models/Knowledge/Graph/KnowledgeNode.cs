public sealed class KnowledgeNode
{
    public required string Id { get; init; }

    public required string Kind { get; init; }

    public required string Name { get; init; }

    public Dictionary<string, string> Properties { get; } = [];

    public HashSet<string> Tags { get; } = [];
}