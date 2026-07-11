namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class InvocationSymbol
{
    public required string Name { get; init; }

    public required string Expression { get; init; }

    public int Line { get; init; }

    public MethodSymbol? TargetMethod { get; set; }
}