using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class FeatureModel
{
    public required string Name { get; init; }

    public List<TypeSymbol> Types { get; } = [];

    public List<MethodSymbol> Methods { get; } = [];

    public List<FlowModel> Flows { get; } = [];

    public List<string> Reasons { get; } = [];
}