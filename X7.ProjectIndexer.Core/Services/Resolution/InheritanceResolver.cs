using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class InheritanceResolver : IRelationshipResolver
{
    public void Resolve(ResolverContext context)
    {
        var semantic = context.Index.Semantic;

        foreach (var child in semantic.Types)
        {
            if (child.BaseTypeSymbol is null)
                continue;

            semantic.Inheritances.Add(new Inheritance
            {
                Child = child,
                Parent = child.BaseTypeSymbol
            });
        }
    }
}