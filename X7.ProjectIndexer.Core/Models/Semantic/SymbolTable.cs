using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Models.Semantic;

public sealed class SymbolTable
{
    public List<ProjectSymbol> Projects { get; } = [];

    public List<TypeSymbol> Types { get; } = [];

    public List<MethodSymbol> Methods { get; } = [];

    public List<PropertySymbol> Properties { get; } = [];

    public List<FieldSymbol> Fields { get; } = [];

    public List<ParameterSymbol> Parameters { get; } = [];

    public List<MethodCall> Calls { get; } = [];

    public List<Reference> References { get; } = [];

    public List<Dependency> Dependencies { get; } = [];

    public List<Inheritance> Inheritances { get; } = [];

    public List<Implementation> Implementations { get; } = [];

    public List<Composition> Compositions { get; } = [];

    public List<Aggregation> Aggregations { get; } = [];

    public List<UsingReference> Usings { get; } = [];

    public List<LocalVariableSymbol> LocalVariables { get; } = [];
    
    public Dictionary<string, TypeSymbol> TypesById { get; } = [];

    public Dictionary<string, TypeSymbol> TypesByName { get; } = [];

    public Dictionary<string, MethodSymbol> MethodsById { get; } = [];
 
    public Dictionary<string, PropertySymbol> PropertiesById { get; } = [];
    
    public Dictionary<string, FieldSymbol> FieldsById { get; } = [];

    public Dictionary<string, List<MethodSymbol>> MethodsByName { get; } = [];

    public SymbolIndex Index { get; } = new();

    public Dictionary<string, MethodScope> ScopesByMethodId { get; } = [];

    public Dictionary<MethodSymbol, List<MethodCall>> CallsByCaller { get; } = [];

    public Dictionary<MethodSymbol, List<MethodCall>> CallsByCallee { get; } = [];

    public Dictionary<TypeSymbol, List<Dependency>> DependenciesBySource { get; } = [];

    public Dictionary<TypeSymbol, List<Dependency>> DependenciesByTarget { get; } = [];
}