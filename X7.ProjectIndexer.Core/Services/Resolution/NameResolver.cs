using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Resolution;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class NameResolver : ISymbolResolver
{
    private readonly SymbolTable _semantic;

    public NameResolver(SymbolTable semantic)
    {
        _semantic = semantic;
    }

    public TypeSymbol? ResolveType(string name)
    {
        return ResolveTypeDetailed(name).Symbol;
    }

    public TypeResolution ResolveTypeDetailed(string name)
    {
        var normalized = Normalize(name);

        if (_semantic.TypesByName.TryGetValue(normalized, out var type))
        {
            return new TypeResolution
            {
                Name = normalized,
                Symbol = type,
                Origin = TypeOrigin.Project
            };
        }

        if (IsExternalType(normalized))
        {
            return new TypeResolution
            {
                Name = normalized,
                Origin = TypeOrigin.External
            };
        }

        return new TypeResolution
        {
            Name = normalized,
            Origin = TypeOrigin.Unknown
        };
    }

    public PropertySymbol? ResolveProperty(
        TypeSymbol type,
        string property)
    {
        return type.Properties.FirstOrDefault(x => x.Name == property);
    }

    public FieldSymbol? ResolveField(
        TypeSymbol type,
        string field)
    {
        return type.Fields.FirstOrDefault(x => x.Name == field);
    }

    public MethodSymbol? ResolveMethod(
        TypeSymbol type,
        string methodName)
    {
        return type.Methods.FirstOrDefault(x => x.Name == methodName);
    }

    public MethodSymbol? ResolveMethod(
        MethodSymbol caller,
        InvocationSymbol invocation)
    {
        if (!_semantic.MethodsByName.TryGetValue(invocation.Name, out var methods))
            return null;

        return methods.FirstOrDefault();
    }

    public TypeSymbol? ResolveVariableType(
        MethodSymbol method,
        string variableName)
    {
        var local = method.Body.LocalVariables
            .FirstOrDefault(x => x.Name == variableName);

        return local?.TypeSymbol;
    }

    private static string Normalize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        name = name.Trim();

        if (name.EndsWith("?"))
            name = name[..^1];

        var genericIndex = name.IndexOf('<');

        if (genericIndex >= 0)
            name = name[..genericIndex];

        return name;
    }

    private static bool IsExternalType(string name)
    {
        return name switch
        {
            "string" => true,
            "object" => true,
            "bool" => true,
            "byte" => true,
            "char" => true,
            "short" => true,
            "ushort" => true,
            "int" => true,
            "uint" => true,
            "long" => true,
            "ulong" => true,
            "float" => true,
            "double" => true,
            "decimal" => true,
            "DateTime" => true,
            "Guid" => true,
            "CancellationToken" => true,
            _ when name.StartsWith("List") => true,
            _ when name.StartsWith("Dictionary") => true,
            _ when name.StartsWith("HashSet") => true,
            _ when name.StartsWith("Queue") => true,
            _ when name.StartsWith("Stack") => true,
            _ when name.StartsWith("Task") => true,
            _ when name.StartsWith("ValueTask") => true,
            _ when name.StartsWith("IEnumerable") => true,
            _ when name.StartsWith("ICollection") => true,
            _ when name.StartsWith("IReadOnly") => true,
            _ when name.StartsWith("IList") => true,
            _ => false
        };
    }
}