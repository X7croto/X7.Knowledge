using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class ImplementationResolver : IRelationshipResolver
{
    public void Resolve(ResolverContext context)
    {
        var semantic = context.Index.Semantic;

        foreach (var type in semantic.Types)
        {
            foreach (var @interface in type.InterfaceSymbols)
            {
                semantic.Implementations.Add(new Implementation
                {
                    Type = type,
                    Interface = @interface
                });
            }
        }
    }
}