using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Semantic;

public sealed class SymbolLookup
{
    private readonly SymbolTable _semantic;

    public SymbolLookup(SymbolTable semantic)
    {
        _semantic = semantic;
    }

    public TypeSymbol? Find(TypeReference? reference)
    {
        if (reference is null)
            return null;

        if (!reference.Resolved)
            return null;

        if (reference.QualifiedName is null)
            return null;

        _semantic.TypesById.TryGetValue(
            reference.QualifiedName,
            out var symbol);

        return symbol;
    }
}