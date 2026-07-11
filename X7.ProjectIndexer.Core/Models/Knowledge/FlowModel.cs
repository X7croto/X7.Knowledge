using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class FlowModel
{
    public required string Name { get; init; }

    public List<FlowStep> Steps { get; } = [];
}