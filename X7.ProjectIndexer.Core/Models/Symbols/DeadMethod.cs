namespace X7.ProjectIndexer.Core.Models.Analysis;

using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class DeadMethod
{
    public required MethodSymbol Method { get; init; }

    public int IncomingCalls { get; set; }
}