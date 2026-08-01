using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class DependencyResolver : IRelationshipResolver
{
    public void Resolve(ResolverContext context)
    {
        var symbolTable = context.Index.Semantic;

        foreach (var type in symbolTable.Types)
        {
            foreach (var field in type.Fields)
                AddDependency(symbolTable, type, field.TypeSymbol, DependencyKind.Field);

            foreach (var property in type.Properties)
                AddDependency(symbolTable, type, property.TypeSymbol, DependencyKind.Property);

            foreach (var method in type.Methods)
            {
                AddDependency(symbolTable, type, method.ReturnTypeSymbol, DependencyKind.ReturnType);

                AddParameterDependencies(symbolTable, type, method.Parameters);

                foreach (var local in method.Body.LocalVariables)
                    AddDependency(symbolTable, type, local.TypeSymbol, DependencyKind.LocalVariable);
            }
        }
    }

    private static void AddParameterDependencies(
        SymbolTable symbolTable,
        TypeSymbol source,
        IEnumerable<ParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
            AddDependency(symbolTable, source, parameter.TypeSymbol, DependencyKind.Parameter);
    }

    private static void AddDependency(
        SymbolTable symbolTable,
        TypeSymbol source,
        TypeSymbol? target,
        DependencyKind kind)
    {
        if (target is null)
            return;

        if (ReferenceEquals(source, target))
            return;

        if (symbolTable.Dependencies.Any(d =>
            ReferenceEquals(d.Source, source) &&
            ReferenceEquals(d.Target, target) &&
            d.Kind == kind))
            return;

        symbolTable.Dependencies.Add(new Dependency2
        {
            Source = source,
            Target = target,
            Kind = kind
        });
    }
}