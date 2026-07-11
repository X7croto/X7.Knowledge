namespace X7.ProjectIndexer.Core.Models.Parsing;

public sealed class UsingDirective
{
    public string Namespace { get; init; } = "";

    public bool IsStatic { get; init; }

    public bool IsGlobal { get; init; }

    public string? Alias { get; init; }
}