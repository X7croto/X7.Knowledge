using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Analysis;

public sealed class ArchitecturalViolation
{
    public required string Rule { get; init; }

    public required string Message { get; init; }

    public TypeSymbol? SourceType { get; init; }

    public TypeSymbol? TargetType { get; init; }

    public MethodSymbol? SourceMethod { get; init; }

    public MethodSymbol? TargetMethod { get; init; }
}