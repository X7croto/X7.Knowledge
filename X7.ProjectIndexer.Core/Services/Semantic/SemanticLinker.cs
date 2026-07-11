using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Semantic;

public sealed class SemanticLinker
{
    public void Link(SymbolTable semantic)
    {
        LinkTypes(semantic);
        LinkFields(semantic);
        LinkProperties(semantic);
        LinkMethods(semantic);
        LinkParameters(semantic);
        LinkLocals(semantic);
    }

    private static void LinkTypes(SymbolTable semantic)
    {
        foreach (var type in semantic.Types)
        {
            if (type.BaseTypeQualifiedName is not null &&
                semantic.TypesById.TryGetValue(type.BaseTypeQualifiedName, out var parent))
            {
                type.BaseTypeSymbol = parent;
            }

            foreach (var iface in type.InterfaceQualifiedNames)
            {
                if (semantic.TypesById.TryGetValue(iface, out var symbol))
                    type.InterfaceSymbols.Add(symbol);
            }
        }
    }

    private static void LinkFields(SymbolTable semantic)
    {
        foreach (var field in semantic.Fields)
        {
            if (field.TypeQualifiedName is null)
                continue;

            if (semantic.TypesById.TryGetValue(field.TypeQualifiedName, out var symbol))
                field.TypeSymbol = symbol;
        }
    }

    private static void LinkProperties(SymbolTable semantic)
    {
        foreach (var property in semantic.Properties)
        {
            if (property.TypeQualifiedName is null)
                continue;

            if (semantic.TypesById.TryGetValue(property.TypeQualifiedName, out var symbol))
                property.TypeSymbol = symbol;
        }
    }

    private static void LinkMethods(SymbolTable semantic)
    {
        foreach (var method in semantic.Methods)
        {
            if (method.ReturnTypeQualifiedName is null)
                continue;

            if (semantic.TypesById.TryGetValue(method.ReturnTypeQualifiedName, out var symbol))
                method.ReturnTypeSymbol = symbol;
        }
    }

    private static void LinkParameters(SymbolTable semantic)
    {
        foreach (var parameter in semantic.Parameters)
        {
            if (parameter.TypeQualifiedName is null)
                continue;

            if (semantic.TypesById.TryGetValue(parameter.TypeQualifiedName, out var symbol))
                parameter.TypeSymbol = symbol;
        }
    }

    private static void LinkLocals(SymbolTable semantic)
    {
        foreach (var local in semantic.LocalVariables)
        {
            if (local.TypeQualifiedName is null)
                continue;

            if (semantic.TypesById.TryGetValue(local.TypeQualifiedName, out var symbol))
                local.TypeSymbol = symbol;
        }
    }
}