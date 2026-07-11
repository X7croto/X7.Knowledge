using X7.ProjectIndexer.Core.Services.Binding;

namespace X7.ProjectIndexer.Core.Models;

public sealed class ParameterNode
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public bool Ref { get; set; }

    public bool Out { get; set; }

    public bool Params { get; set; }

    public bool Optional { get; set; }

    public TypeReference? TypeReference { get; set; }
}