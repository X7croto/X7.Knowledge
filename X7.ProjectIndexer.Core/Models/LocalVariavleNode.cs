using X7.ProjectIndexer.Core.Services.Binding;

namespace X7.ProjectIndexer.Core.Models;

public sealed class LocalVariableNode
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public TypeReference? TypeReference { get; set; }
}