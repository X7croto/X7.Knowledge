using X7.ProjectIndexer.Core.Models.Relations;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class CompositionResolver
{
    public void Resolve(SymbolTable semantic)
    {
        foreach (var type in semantic.Types)
        {
            foreach (var field in type.Fields)
            {
                if (field.TypeSymbol is null)
                    continue;

                if (field.TypeSymbol == type)
                    continue;

                if (field.Type.EndsWith("[]"))
                    continue;

                semantic.Compositions.Add(new Composition
                {
                    Owner = type,
                    Part = field.TypeSymbol
                });
            }
        }

        RemoveDuplicates(semantic);
    }

    private static void RemoveDuplicates(SymbolTable semantic)
    {
        var unique = semantic.Compositions
            .GroupBy(x => (x.Owner.Id, x.Part.Id))
            .Select(x => x.First())
            .ToList();

        semantic.Compositions.Clear();
        semantic.Compositions.AddRange(unique);
    }
}