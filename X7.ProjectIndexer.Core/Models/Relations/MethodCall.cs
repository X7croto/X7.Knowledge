using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Relations;

public sealed class MethodCall
{
    public required MethodSymbol Caller { get; init; }

    public MethodSymbol? Callee { get; init; }

    public required InvocationSymbol Invocation { get; init; }
}