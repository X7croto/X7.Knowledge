using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class AggregationResolver
{
    public void Resolve(SymbolTable semantic)
    {
        foreach (var type in semantic.Types)
        {
            foreach (var property in type.Properties)
            {
                if (property.TypeSymbol is null)
                    continue;

                if (property.TypeSymbol == type)
                    continue;

                semantic.Aggregations.Add(new Aggregation
                {
                    Owner = type,
                    Part = property.TypeSymbol
                });
            }
        }

        RemoveDuplicates(semantic);
    }

    private static void RemoveDuplicates(SymbolTable semantic)
    {
        var unique = semantic.Aggregations
            .GroupBy(x => (x.Owner.Id, x.Part.Id))
            .Select(x => x.First())
            .ToList();

        semantic.Aggregations.Clear();
        semantic.Aggregations.AddRange(unique);
    }
}