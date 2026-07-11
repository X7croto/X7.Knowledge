using X7.ProjectIndexer.Core.Services.Binding;

namespace X7.ProjectIndexer.Core.Models;

public sealed class PropertyNode
{
    public required string Id { get; init; }

    public required string TypeId { get; init; }

    public required string Name { get; init; }

    public required string Type { get; init; }

    public bool HasGetter { get; set; }

    public bool HasSetter { get; set; }

    public bool InitOnly { get; set; }

    public string Accessibility { get; set; } = "";

    public TypeReference? TypeReference { get; set; }
}