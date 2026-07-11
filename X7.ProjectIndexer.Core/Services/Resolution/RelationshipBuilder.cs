using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class RelationshipBuilder
{
    public void Build(ProjectIndexOld index)
    {
        var context = new ResolverContext(index);

        IRelationshipResolver[] resolvers =
        [
            new InheritanceResolver(),
            new ImplementationResolver(),
            new DependencyResolver(),
            new MethodCallResolver()
        ];

        foreach (var resolver in resolvers)
        {
            resolver.Resolve(context);
        }
    }
}