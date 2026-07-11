namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class MemberAccessSymbol
{
    public required string Expression { get; init; }

    public required string Member { get; init; }

    public int Line { get; init; }

    public ISymbol? TargetSymbol { get; set; }
}