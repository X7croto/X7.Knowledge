using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class FlowStep
{
    public required MethodSymbol Method { get; init; }

    public int Order { get; init; }

    public string Role { get; init; } = "";
}