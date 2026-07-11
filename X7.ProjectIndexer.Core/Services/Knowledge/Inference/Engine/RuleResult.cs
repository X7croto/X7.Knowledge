namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

public sealed class RuleResult
{
    public required string Rule { get; init; }

    public int Confidence { get; init; }

    public string Reason { get; init; } = "";
}