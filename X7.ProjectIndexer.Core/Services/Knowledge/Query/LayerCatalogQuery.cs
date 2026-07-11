using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public sealed class LayerCatalogQuery
    : IKnowledgeQuery<List<string>>
{
    public List<string> Execute(ProjectIndexOld index)
    {
        return index.Knowledge.Architecture.Services
            .Select(x => x.Layer)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }
}