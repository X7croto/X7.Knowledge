using System.Reflection;
using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Services.Binding;

namespace X7.ProjectIndexer.Core.Models;

public sealed class MethodNode
{
    public required string Id { get; init; }

    public required string TypeId { get; init; }

    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public string Accessibility { get; set; } = "";

    public bool Static { get; set; }

    public bool Virtual { get; set; }

    public bool Override { get; set; }

    public bool Abstract { get; set; }

    public bool Async { get; set; }

    public List<ParameterNode> Parameters { get; } = [];

    public List<string> Attributes { get; } = [];

    public BlockNode Body { get; } = new();

    public TypeReference? ReturnTypeReference { get; set; }
}