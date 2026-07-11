using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Parsing;

public sealed class SourceFile
{
    public required string Path { get; init; }

    public required string RelativePath { get; init; }

    public string? Namespace { get; set; }

    public List<UsingDirective> Usings { get; } = [];

    public List<TypeNode> Types { get; } = [];

    public Dictionary<string, string> Aliases { get; } = [];

    public bool HasGlobalUsings { get; set; }

    public List<string> StaticUsings { get; } = [];

}