using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Services.Resolution;

public sealed class MethodCallResolver : IRelationshipResolver
{
    public void Resolve(ResolverContext context)
    {
        var semantic = context.Index.Semantic;
        var lookup = context.Lookup;

        foreach (var type in semantic.Types)
        {
            foreach (var method in type.Methods)
            {
                foreach (var invocation in method.Body.Invocations)
                {
                    var target =
                        lookup.FindMethod(
                            invocation.Name,
                            method);

                    if (target is null)
                        continue;

                    semantic.Calls.Add(new MethodCall
                    {
                        Caller = method,
                        Callee = target,
                        Invocation = invocation
                    });
                }
            }
        }
    }
}