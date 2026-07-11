using X7.ProjectIndexer.Core.Models.Semantic;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class LocalVariableResolver
{
    private readonly ISymbolResolver _resolver;

    public LocalVariableResolver(ISymbolResolver resolver)
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