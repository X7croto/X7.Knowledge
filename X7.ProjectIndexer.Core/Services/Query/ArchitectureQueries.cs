using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Query;

public static class ArchitectureQueries
{
    public static IEnumerable<TypeSymbol> HighlyCoupled(
        this IEnumerable<TypeSymbol> source,
        int minimum = 10)
    {
        return source.Where(x => x.FanIn + x.FanOut >= minimum);
    }

    public static IEnumerable<TypeSymbol> Abstract(
        this IEnumerable<TypeSymbol> source)
    {
        return source.Where(x => x.Abstract);
    }

    public static IEnumerable<TypeSymbol> Concrete(
        this IEnumerable<TypeSymbol> source)
    {
        return source.Where(x => !x.Abstract);
    }

    public static IEnumerable<TypeSymbol> DistanceGreaterThan(
        this IEnumerable<TypeSymbol> source,
        double value)
    {
        return source.Where(x => x.DistanceFromMainSequence > value);
    }
}