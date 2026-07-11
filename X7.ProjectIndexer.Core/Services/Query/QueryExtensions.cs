using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Query;

public static class QueryExtensions
{
    public static IEnumerable<TypeSymbol> Layer(
        this IEnumerable<TypeSymbol> source,
        int layer)
    {
        return source.Where(x => x.Layer == layer);
    }

    public static IEnumerable<TypeSymbol> Roots(
        this IEnumerable<TypeSymbol> source)
    {
        return source.Where(x => x.IsRoot);
    }

    public static IEnumerable<TypeSymbol> Leaves(
        this IEnumerable<TypeSymbol> source)
    {
        return source.Where(x => x.IsLeaf);
    }

    public static IEnumerable<TypeSymbol> Stable(
        this IEnumerable<TypeSymbol> source,
        double max = 0.3)
    {
        return source.Where(x => x.Instability <= max);
    }

    public static IEnumerable<TypeSymbol> Unstable(
        this IEnumerable<TypeSymbol> source,
        double min = 0.7)
    {
        return source.Where(x => x.Instability >= min);
    }

    public static IEnumerable<MethodSymbol> DeadCode(
        this IEnumerable<MethodSymbol> source)
    {
        return source.Where(x => x.IsDeadCode);
    }

    public static IEnumerable<MethodSymbol> Recursive(
        this IEnumerable<MethodSymbol> source)
    {
        return source.Where(x => x.Recursive);
    }

    public static IEnumerable<MethodSymbol> EntryPoints(
        this IEnumerable<MethodSymbol> source)
    {
        return source.Where(x => x.IsEntryPoint);
    }
}