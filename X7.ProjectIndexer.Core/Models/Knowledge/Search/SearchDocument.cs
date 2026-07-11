public sealed class SearchDocument
{
    public required string Id { get; init; }

    public required string Kind { get; init; }

    public required string Name { get; init; }

    public required string Namespace { get; init; }

    public string Summary { get; set; } = "";

    public HashSet<string> Tokens { get; } = [];

    public HashSet<string> Keywords { get; } = [];

    public HashSet<string> Related { get; } = [];
}