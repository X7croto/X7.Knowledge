using X7.ProjectIndexer.Core.Models.Semantic;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class TypeReferenceResolver
{
    private readonly ISymbolResolver _resolver;

    public TypeReferenceResolver(ISymbolResolver resolver)
    {
        _resolver = resolver;
    }

    public void Resolve(SymbolTable semantic)
    {
        foreach (var method in semantic.Methods)
        {
            foreach (var local in method.Body.LocalVariables)
            {
                local.TypeSymbol = _resolver.ResolveType(local.Type);
            }
        }
    }
}