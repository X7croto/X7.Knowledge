using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class MethodSymbol : BaseSymbol
{
    public required string ReturnType { get; init; }

    public string Accessibility { get; init; } = "";

    public bool Static { get; init; }

    public bool Virtual { get; init; }

    public bool Override { get; init; }

    public bool Abstract { get; init; }

    public bool Async { get; init; }

    public List<ParameterSymbol> Parameters { get; } = [];

    public List<string> Attributes { get; } = [];

    public required TypeSymbol DeclaringType { get; init; }

    public MethodBodySymbol Body { get; } = new();

    public TypeSymbol? ReturnTypeSymbol { get; set; }

    public int FanIn { get; set; }

    public int FanOut { get; set; }

    public bool Recursive { get; set; }

    public bool IsDeadCode { get; set; }

    public int MaxCallDepth { get; set; }

    public bool IsEntryPoint { get; set; }

    public bool IsFrameworkMethod { get; set; }

    public bool IsTestMethod { get; set; }

    public bool IsAsyncBoundary { get; set; }

    public IReadOnlyCollection<MethodSymbol> Callers =>
        _callers;

    public IReadOnlyCollection<MethodSymbol> Callees =>
        _callees;

    private readonly HashSet<MethodSymbol> _callers = [];

    private readonly HashSet<MethodSymbol> _callees = [];

    public void AddCaller(MethodSymbol method)
    {
        _callers.Add(method);
    }

    public void AddCallee(MethodSymbol method)
    {
        _callees.Add(method);
    }

    public string? ReturnTypeQualifiedName { get; set; }
}