using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Builders;

public sealed class NamespaceIndexBuilder
{
    public IEnumerable<NamespaceIndex> Build(ProjectIndexOld index)
    {
        foreach (var group in index.Semantic.Types.GroupBy(x => x.Namespace))
        {
            var ns = new NamespaceIndex
            {
                Name = group.Key
            };

            ns.Types.AddRange(group.Select(x => x.Name));

            yield return ns;
        }
    }
}