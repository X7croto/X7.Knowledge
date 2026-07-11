using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class DependencyChain
{
    public IReadOnlyList<TypeSymbol> Path => _path;

    private readonly List<TypeSymbol> _path = [];

    public void Add(TypeSymbol type)
    {
        _path.Add(type);
    }

    public void AddRange(IEnumerable<TypeSymbol> types)
    {
        _path.AddRange(types);
    }
}