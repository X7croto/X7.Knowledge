using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Semantic;

public sealed class SymbolContext
{
    public required SymbolTable Semantic { get; init; }

    public required ArchitectureModel Architecture { get; init; }

    public required ProjectIndexOld Index { get; init; }
}