using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Query;

public sealed class SymbolQuery
{
    private readonly SymbolTable _semantic;

    public SymbolQuery(SymbolTable semantic)
    {
        _semantic = semantic;
    }

    public IEnumerable<TypeSymbol> Types =>
        _semantic.Types;

    public IEnumerable<MethodSymbol> Methods =>
        _semantic.Methods;

    public IEnumerable<PropertySymbol> Properties =>
        _semantic.Properties;

    public IEnumerable<FieldSymbol> Fields =>
        _semantic.Fields;
}