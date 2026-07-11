using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public sealed class ServiceCatalogQuery
    : IKnowledgeQuery<List<ServiceModel>>
{
    public List<ServiceModel> Execute(ProjectIndexOld index)
    {
        return index.Knowledge.Architecture.Services
            .OrderBy(x => x.Name)
            .ToList();
    }
}