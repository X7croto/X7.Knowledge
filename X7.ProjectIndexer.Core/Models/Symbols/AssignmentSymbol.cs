namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class AssignmentSymbol
{
    public required string LeftExpression { get; init; }

    public required string RightExpression { get; init; }

    public int Line { get; init; }

    public ISymbol? LeftSymbol { get; set; }

    public ISymbol? RightSymbol { get; set; }
}