using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Semantic;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class ResolverContext
{
    public ProjectIndexOld Index { get; }

    public SymbolTable Semantic => Index.Semantic;

    public SymbolLookup Lookup { get; }

    public ResolverContext(ProjectIndexOld index)
    {
        Index = index;
        Lookup = new SymbolLookup(index.Semantic);
    }

    public IEnumerable<TypeSymbol> Types =>
        Semantic.Types;

    public IEnumerable<MethodSymbol> Methods =>
        Semantic.Methods;
}