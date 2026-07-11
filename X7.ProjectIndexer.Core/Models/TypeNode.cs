using X7.ProjectIndexer.Core.Services.Binding;

namespace X7.ProjectIndexer.Core.Models;

public sealed class TypeNode
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Namespace { get; init; }

    public required string Kind { get; init; }

    public string Accessibility { get; set; } = "";

    public bool Partial { get; set; }   

    public bool Static { get; set; }

    public bool Abstract { get; set; }

    public bool Record { get; set; }

    public string? BaseType { get; set; }

    public List<string> Interfaces { get; } = [];

    public List<string> Attributes { get; } = [];

    public List<MethodNode> Methods { get; } = [];

    public List<PropertyNode> Properties { get; } = [];

    public List<FieldNode> Fields { get; } = [];

    public TypeReference? BaseTypeReference { get; set; }

    public List<TypeReference> InterfaceReferences { get; } = [];
}