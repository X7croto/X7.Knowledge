namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class MethodBodySymbol
{
    public List<ParameterSymbol> Parameters { get; } = [];

    public List<LocalVariableSymbol> LocalVariables { get; } = [];

    public List<IdentifierSymbol> Identifiers { get; } = [];

    public List<InvocationSymbol> Invocations { get; } = [];

    public List<ObjectCreationSymbol> ObjectCreations { get; } = [];

    public List<MemberAccessSymbol> MemberAccesses { get; } = [];

    public List<AssignmentSymbol> Assignments { get; } = [];

    public List<ReturnSymbol> Returns { get; } = [];
}