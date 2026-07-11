using X7.ProjectIndexer.Core.Services.Binding;

namespace X7.ProjectIndexer.Core.Models;

public sealed class FieldNode
{
    public required string Id { get; init; }

    public required string TypeId { get; init; }

    public required string Name { get; init; }

    public required string Type { get; init; }

    public string Accessibility { get; set; } = "";

    public bool Static { get; set; }

    public bool Readonly { get; set; }

    public bool Const { get; set; }

    public TypeReference? TypeReference { get; set; }
}