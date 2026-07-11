using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class SymbolLookup
{
    private readonly SymbolTable _semantic;

    public SymbolLookup(SymbolTable semantic)
    {
        _semantic = semantic;
    }

    public TypeSymbol? Find(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        if (_semantic.TypesById.TryGetValue(fullName, out var type))
            return type;

        if (_semantic.TypesByName.TryGetValue(fullName, out type))
            return type;

        var simple =
            fullName.Split('.').Last();

        _semantic.TypesByName.TryGetValue(simple, out type);

        return type;
    }

    public IEnumerable<TypeSymbol> FindAll(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Enumerable.Empty<TypeSymbol>();

        return _semantic.Types
            .Where(x =>
                x.Name == name ||
                x.Id == name);
    }

    public MethodSymbol? FindMethod(string id)
    {
        _semantic.MethodsById.TryGetValue(id, out var method);
        return method;
    }
    public MethodSymbol? FindMethod(
        string name,
        MethodSymbol caller)
    {
        if (_semantic.MethodsByName.TryGetValue(name, out var methods))
        {
            if (methods.Count == 1)
                return methods[0];

            //----------------------------------------
            // Prioriza métodos do mesmo tipo
            //----------------------------------------

            var sameType =
                methods.FirstOrDefault(m =>
                    ReferenceEquals(
                        m.DeclaringType,
                        caller.DeclaringType));

            if (sameType != null)
                return sameType;

            //----------------------------------------
            // depois mesmo namespace
            //----------------------------------------

            var sameNamespace =
                methods.FirstOrDefault(m =>
                    m.DeclaringType.Namespace ==
                    caller.DeclaringType.Namespace);

            if (sameNamespace != null)
                return sameNamespace;

            //----------------------------------------
            // fallback
            //----------------------------------------

            return methods[0];
        }

        return null;
    }
}